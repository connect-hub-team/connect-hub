using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Core.Dto.Chat;

public class ChainUser
{

  public record struct DataContext(
    bool IsMuted = false,
    bool IsArchived = false
  );
  
  public Guid Id { get; set; }
  public string UserName { get; set; } // user's name in auth server
  public virtual Chain Chain { get; set; }
  [Column(TypeName = "jsonb")]
  public DataContext Data { get; set; }
}