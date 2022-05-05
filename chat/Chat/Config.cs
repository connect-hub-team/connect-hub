using RabbitHub.Config;

#nullable disable
namespace Chat.Chat;
public class Config
{
  public ConnectionConfig Connection { get; set; }
  public QueueConfig Queue { get; set; }
}