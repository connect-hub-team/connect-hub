using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Core.Dto.Chat;

/// <summary>
/// messages chain (dialog or group chat)
/// </summary>
public class Chain
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public ChainType Type { get; set; }
  [Column(TypeName = "jsonb")]
  public ChainContext Context { get; set; }
  public virtual IEnumerable<ChainUser> Users { get; set; } 
    = new List<ChainUser>();
}