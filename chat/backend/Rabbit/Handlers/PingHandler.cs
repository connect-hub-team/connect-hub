using RabbitHub.Handlers;
using Chat.Core.Dto;
using RabbitHub;

namespace Chat.Rabbit.Handlers;

public class PingHandler : RpcHandler<PingDto>
{
  public static readonly string Topic = "chat.ping.rpc";
  public override async Task<IHandleResult> Handle(Message message)
  {
    var ping = new PingDto("pong", DateTime.Now);

    return Ack(ping);
  }
}