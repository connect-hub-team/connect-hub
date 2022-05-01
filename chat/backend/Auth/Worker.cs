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
    await context.Database.EnsureCreatedAsync(cancellationToken);
    
    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    // add clients
    if (await applicationManager.FindByClientIdAsync("angular-client", cancellationToken) is null)
    {
      await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor()
      {
        ClientId = "angular-client",
        RedirectUris =
        {
          new Uri("https://localhost:8080"),
          new Uri("http://localhost:8080"),
          new Uri("http://localhost:7000")
        },
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
    if (await scopeManager.FindByNameAsync("chat", cancellationToken) is null)
    {
      await scopeManager.CreateAsync(new OpenIddictScopeDescriptor()
      {
        Name = "chat",
        Resources = {"chat"}
      }, cancellationToken);
    }

  }

  public Task StopAsync(CancellationToken cancellationToken)
    => Task.CompletedTask;
}