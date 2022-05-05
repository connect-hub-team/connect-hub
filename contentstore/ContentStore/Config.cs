using RabbitHub.Config;
using Swashbuckle.AspNetCore.SwaggerUI;

#nullable disable
namespace ContentStore;
public class Config
{
  public string Urls { get; set; }
  public ConnectionConfig Connection { get; set; }
  public QueueConfig Queue { get; set; }
}