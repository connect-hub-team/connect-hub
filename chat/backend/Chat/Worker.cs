using Chat.Chat.Handlers;
using Rabbit = RabbitHub;

namespace Chat.Chat;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly Rabbit.Hub _hub;

  public Worker(ILogger<Worker> logger, Rabbit.Hub hub)
  {
    _logger = logger;
    _hub = hub;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var response = await _hub.Rpc(new Rabbit.Message(), PingHandler.Topic);
      _logger.LogInformation("Ping at: {time} {text}", DateTimeOffset.Now, response?.Text);
      await Task.Delay(1000, stoppingToken);
    }
  }
}
