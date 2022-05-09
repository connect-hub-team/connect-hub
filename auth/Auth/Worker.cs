using System.Reflection;
using Chat.Auth.Data;
using FastConfig;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace Chat.Auth;

public class Worker : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public Worker(IServiceProvider serviceProvider)
    => _serviceProvider = serviceProvider;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await using var scope = this._serviceProvider.CreateAsyncScope();

    var config = await scope.ServiceProvider.GetService<FastConfigClient>()?.Get<Config>()
                 ?? throw new ApplicationException("");
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await context.Database.EnsureCreatedAsync(cancellationToken);

    // TODO: implement clients deletion

    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    foreach (var client in config.Clients)
    {
      // HACK:
      // крч, приходиться вот так вот костылить с рефлексией и скрытыми полями
      // Дело в том, что у OpenIddictApplicationDescription многие поля
      // (Permission и RedirectUris например) не имеют сеттера и при парсинге
      // из жсона они не мапятся. Если же отнаследоваться от класса и добавить
      // сеттер, то оно мапится и нормально передается в метод CreateAsync/UpdateAsync.
      // Но при апкасте значения перекрывающих пропов теряется.
      var desc = new OpenIddictApplicationDescriptor
      {
        ClientId = client.ClientId,
        DisplayName = client.DisplayName,
        Type = client.Type,
      };
      desc
        .GetType()
        .GetField("<Permissions>k__BackingField",
          BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(desc, client.Permissions);
      typeof(OpenIddictApplicationDescriptor)
        .GetField("<RedirectUris>k__BackingField",
          BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(desc, client.RedirectUris);
      
      if (await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken) is 
          OpenIddictEntityFrameworkCoreApplication model)
        await applicationManager.UpdateAsync(model, desc, cancellationToken);
      else
        await applicationManager.CreateAsync(desc, cancellationToken);
    }

    var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    foreach (var scopeDescriptor in config.Scopes)
    {
      // HACK
      var scopeDesc = new OpenIddictScopeDescriptor
      {
        Name = scopeDescriptor.Name,
        DisplayName = scopeDescriptor.DisplayName,
      };
      scopeDesc
        .GetType()
        .GetField("<Resources>k__BackingField",
          BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(scopeDesc, scopeDescriptor.Resources);
      
      if (await scopeManager.FindByNameAsync(scopeDescriptor.Name!, cancellationToken)
          is OpenIddictEntityFrameworkCoreScope efScope)
        await scopeManager.UpdateAsync(efScope, scopeDesc, cancellationToken);
      else
        await scopeManager.CreateAsync(scopeDesc, cancellationToken);
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
    => Task.CompletedTask;
}