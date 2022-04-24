using System.Security.Claims;
using Chat.Auth.Data;
using Chat.Auth.Data.Dtos;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Chat.Auth.Controllers;

public class AuthController : Controller
{
  private readonly IOpenIddictApplicationManager _applicationManager;
  private readonly IOpenIddictAuthorizationManager _authorizationManager;
  private readonly IOpenIddictScopeManager _scopeManager;
  private readonly SignInManager<User> _signInManager;
  private readonly UserManager<User> _userManager;

  public AuthController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager,
    SignInManager<User> signInManager,
    UserManager<User> userManager)
  {
    _applicationManager = applicationManager;
    _authorizationManager = authorizationManager;
    _scopeManager = scopeManager;
    _signInManager = signInManager;
    _userManager = userManager;
  }

  [Route("~/auth")]
  [IgnoreAntiforgeryToken]
  public async Task<IActionResult> Authorize()
  {
    var request = HttpContext.GetOpenIddictServerRequest() ??
                  throw new InvalidOperationException("OIDC request can't be retrieved");

    if (request.HasPrompt(Prompts.Login))
    {
      var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));
      
      var parameters = Request.HasFormContentType ?
        Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
        Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();
      
      parameters.Add(new KeyValuePair<string, StringValues>(Parameters.Prompt, new(prompt)));

      return Challenge(
        authenticationSchemes: IdentityConstants.ApplicationScheme,
        properties: new AuthenticationProperties()
        {
          RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
        }
      );
    }

    var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    if (result == null || 
        !result.Succeeded || 
        (request.MaxAge != null && result.Properties?.IssuedUtc != null
         && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
    {
      if (request.HasPrompt(Prompts.None))
        return Forbid(
          authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
          properties: new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "You need to login",
          }!)
        );

      return Challenge(
        authenticationSchemes: IdentityConstants.ApplicationScheme,
        properties:new AuthenticationProperties()
        {
          RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
        }
      );
    }
  
    // find user
    var user = await _userManager.GetUserAsync(result.Principal) ??
               throw new InvalidOperationException("Can't find user");
    
    // find application info
    var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                      throw new InvalidOperationException("Can't find client app info");

    var authorizations = await _authorizationManager.FindAsync(
      subject: await _userManager.GetUserIdAsync(user),
      client: await _applicationManager.GetIdAsync(application),
      status: Statuses.Valid,
      type: AuthorizationTypes.Permanent,
      scopes: request.GetScopes()).ToListAsync();

      switch (await _applicationManager.GetConsentTypeAsync(application))
      {
        // If the consent is external (e.g when authorizations are granted by a sysadmin),
        // immediately return an error if no authorization can be found in the database.
        case ConsentTypes.External when !authorizations.Any():
          return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
              [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
              [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                "The logged in user is not allowed to access this client application."
            }!)
          );

        // If the consent is implicit or if an authorization was found,
        // return an authorization response without displaying the consent form.
        case ConsentTypes.Implicit:
        case ConsentTypes.External when authorizations.Any():
        case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
          var principal = await _signInManager.CreateUserPrincipalAsync(user);

          // Note: in this sample, the granted scopes match the requested scope
          // but you may want to allow the user to uncheck specific scopes.
          // For that, simply restrict the list of scopes before calling SetScopes.
          principal.SetScopes(request.GetScopes());
          principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

          // Automatically create a permanent authorization to avoid requiring explicit consent
          // for future authorization or token requests containing the same scopes.
          var authorization = authorizations.LastOrDefault() ??
                              await _authorizationManager.CreateAsync(
                                principal: principal,
                                subject: await _userManager.GetUserIdAsync(user),
                                client: await _applicationManager.GetIdAsync(application) ?? string.Empty,
                                type: AuthorizationTypes.Permanent,
                                scopes: principal.GetScopes()
                              );

          principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

          foreach (var claim in principal.Claims)
          {
              claim.SetDestinations(GetDestinations(claim, principal));
          }

          return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

          // At this point, no authorization was found in the database and an error must be returned
          // if the client application specified prompt=none in the authorization request.
          case ConsentTypes.Explicit   when request.HasPrompt(Prompts.None):
          case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
              return Forbid(
                  authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                  properties: new AuthenticationProperties(new Dictionary<string, string>
                  {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                      "Interactive user consent is required."
                  }!));

          // In every other case, render the consent form.
          default:
              return View(new AuthorizeDto()
              {
                  ApplicationName = await _applicationManager.GetDisplayNameAsync(application) ?? "",
                  Scope = request.Scope ?? ""
              });
      }

    return null;
  }

  private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
  {
    switch (claim.Type)
    {
      case Claims.Name:
        yield return Destinations.AccessToken;

        if (principal.HasScope(Scopes.Profile))
          yield return Destinations.IdentityToken;

        yield break;

      case Claims.Email:
        yield return Destinations.AccessToken;

        if (principal.HasScope(Scopes.Email))
          yield return Destinations.IdentityToken;

        yield break;

      case Claims.Role:
        yield return Destinations.AccessToken;

        if (principal.HasScope(Scopes.Roles))
          yield return Destinations.IdentityToken;

        yield break;

      // Never include the security stamp in the access and identity tokens, as it's a secret value.
      case "AspNet.Identity.SecurityStamp": yield break;

      default:
        yield return Destinations.AccessToken;
        yield break;
    }
  }
}