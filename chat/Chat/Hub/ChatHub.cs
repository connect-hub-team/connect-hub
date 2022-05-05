using Microsoft.AspNetCore.Authorization;
using SignalR = Microsoft.AspNetCore.SignalR;

namespace Chat.Chat.Hub;

[Authorize]
public class ChatHub : SignalR.Hub
{
  public ChatHub() { }

  public override Task OnConnectedAsync()
  {
    return base.OnConnectedAsync();
  }

  public override Task OnDisconnectedAsync(Exception? exception)
  {
    return base.OnDisconnectedAsync(exception);
  }
}