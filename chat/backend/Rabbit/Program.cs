using Chat.Rabbit;
using Chat.Rabbit.Handlers;
using FastConfig;
using RabbitHub;
using RabbitHub.Config;

string appId = "chat";
var fastConfig = FastConfigClient.FromEnvironment(appId: appId);
var config = await fastConfig.Get<Config>() ?? throw new ArgumentNullException();

IHost host = Host.CreateDefaultBuilder(args)
  .ConfigureServices(services =>
  {
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
  .Build();

await host.RunAsync();
