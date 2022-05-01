using Chat.Chat;
using Chat.Chat.Handlers;
using Chat.Chat.Hub;
using Core.Extensions;
using FastConfig;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

const string appId = "chat";
var fastConfig = FastConfigClient.FromEnvironment(appId: appId);
var config = await fastConfig.Get<Config>() ?? throw new ArgumentNullException();

var host = WebHost.CreateDefaultBuilder(args)
  .ConfigureServices(services =>
  {
    services.AddConnectHubAuth(appId);
    services.AddSignalR();
    services.AddHostedService<Worker>();
    services.AddRabbitHub(hub =>
      hub
      .Connect(config.Connection)
      .UseDefaultConsumer(cons =>
        cons
        .Queue(config.Queue)
        .DeclareQueue()
        .BindTopics()
        .HandleRpc<PingHandler>(PingHandler.Topic)
    ));
  })
  .Configure(builder =>
  {
    builder.UseAuthentication();
    builder.UseAuthorization();

    builder.UseRouting();
    builder.UseEndpoints(endpoints
      => endpoints.MapHub<ChatHub>("/ws"));
  })
  .Build();

await host.StartAsync();
