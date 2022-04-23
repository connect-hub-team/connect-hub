using System.Security.Claims;
using Chat.Auth.Data;
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
    var user = _userManager.GetUserAsync(result.Principal) ??
               throw new InvalidOperationException("Can't find user");
    
    // find application info
    var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                      throw new InvalidOperationException("Can't find client app info");


    return null;
  }
}