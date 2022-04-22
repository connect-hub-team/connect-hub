using System.Security.Claims;
using Chat.Auth.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Chat.Auth.Controllers;

public class AuthController : Controller
{
  private readonly IOpenIddictScopeManager _manager;
  private readonly AuthDbContext _context;

  public AuthController(AuthDbContext context, IOpenIddictScopeManager manager)
  {
    _context = context;
    _manager = manager;
  }

  [Route("~/auth")]
  public async Task<IResult> Authorize()
  {
    var request = HttpContext.GetOpenIddictServerRequest();

    var identifier = (string?) request["identity_id"];
    var user = await _context.FindAsync<User>(identifier);
    if (user is null)
    {
      return
      Results.Challenge(
        authenticationSchemes: new [] {OpenIddictServerAspNetCoreDefaults.AuthenticationScheme},
        properties: new AuthenticationProperties(new Dictionary<string, string>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified identity is invalid."
        }!));
    }

    var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
    identity.AddClaims(new[]
    {
      new Claim(Claims.Email, user.Email),
      new Claim(Claims.Username, user.UserName),
    });

    var principal = new ClaimsPrincipal(identity);

    principal.SetScopes(request.GetScopes());
    principal.SetResources(await _manager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

    return Results.SignIn(principal, properties: null, 
      OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }
}