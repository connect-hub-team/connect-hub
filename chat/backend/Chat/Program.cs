using Chat.Chat;
using Chat.Chat.Handlers;
using Chat.Chat.Hub;
using Core.Extensions;
using FastConfig;
using Microsoft.AspNetCore;

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

    builder.UseRouting();

    builder.UseAuthorization();

    builder.UseEndpoints(endpoints
      => endpoints.MapHub<ChatHub>("/ws"));
  })
  .Build();

await host.StartAsync();
