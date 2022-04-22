using Chat.Auth.Data;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Chat.Auth;

public class Worker : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public Worker(IServiceProvider serviceProvider)
    => _serviceProvider = serviceProvider;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await using var scope = this._serviceProvider.CreateAsyncScope();

    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    // add clients
    if (await applicationManager.FindByClientIdAsync("angular-client") is null)
    {
      await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor()
      {
        ClientId = "angular-client",
        RedirectUris = {new Uri("https://localhost:8080")},
        Permissions =
        {
          Permissions.Endpoints.Authorization,
          Permissions.Endpoints.Token,
          Permissions.GrantTypes.AuthorizationCode,
          Permissions.ResponseTypes.Code,
          Permissions.Scopes.Email,
          Permissions.Scopes.Profile,
          Permissions.Scopes.Roles,
          // here we'll add scope for microservice access, e.g.
          //Permissions.Prefixes.Scope + "chat",
          //Permissions.Prefixes.Scope + "calls",
          //Permissions.Prefixes.Scope + "content",
        }
      });
    }

    var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    // add resources
    if (await scopeManager.FindByNameAsync("chat") is null)
    {
      await scopeManager.CreateAsync(new OpenIddictScopeDescriptor()
      {
        Name = "Chat",
        Resources = {"Chat"}
      });
    }

  }

  public Task StopAsync(CancellationToken cancellationToken)
    => Task.CompletedTask;
}