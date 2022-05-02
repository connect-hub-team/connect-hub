using Chat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Chat.Data;

public class ChatDbContext : DbContext
{
  public DbSet<Chain> Chains { get; set; }
  public DbSet<Message> Messages { get; set; }
  public DbSet<ChainUser> Users { get; set; }
  
  private readonly string connectionUrl;
  public ChatDbContext(string connectionUrl)
  {
    this.connectionUrl = connectionUrl;
  }

  public ChatDbContext()
  {
  }
  
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseLazyLoadingProxies();
    optionsBuilder.UseNpgsql(connectionUrl);
    base.OnConfiguring(optionsBuilder);
  }
}