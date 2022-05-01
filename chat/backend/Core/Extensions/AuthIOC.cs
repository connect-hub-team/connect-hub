using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;

namespace Core.Extensions;

public static class AuthIOC
{
  public static IServiceCollection AddConnectHubAuth(this IServiceCollection services, string appId)
  {
    services.AddOpenIddict()
      .AddValidation(options =>
      {
        // TODO: use FastConfig

        options.SetIssuer("https://localhost:7671");
        options.AddAudiences(appId);

        options.AddEncryptionKey(new SymmetricSecurityKey(
          Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        options.UseSystemNetHttp();
        options.UseAspNetCore();
      });
    
    services.AddAuthentication(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    services.AddAuthorization();
    
    return services;
  }
}