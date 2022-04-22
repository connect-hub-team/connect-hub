using RabbitHub.Config;

#nullable disable
namespace Chat.Rabbit;
public class Config
{
  public ConnectionConfig Connection { get; set; }
  public QueueConfig Queue { get; set; }
}