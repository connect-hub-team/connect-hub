using Microsoft.EntityFrameworkCore.Design;

namespace Chat.Chat.Data;

public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
  public ChatDbContext CreateDbContext(string[] args)
  {
    // call FastConfig
    // TODO: hardcoded
    var connUrl = "Server=127.0.0.1;Port=5432;Database=chatdb;User Id=chat;Password=password123;";
    return new ChatDbContext(connUrl);
  }
}