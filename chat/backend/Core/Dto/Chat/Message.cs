using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Core.Dto.Chat;

public class Message
{
  public record struct ContentContext(
    string text,
    bool seen = false
  );
  
  public Guid Id { get; set; }
  public virtual Chain Chain { get; set; }
  [Column(TypeName = "jsonb")]
  public ContentContext Content { get; set; }
  public DateTime SentAt { get; set; } = DateTime.Now;
  public MessageType Type { get; set; } = MessageType.Message;
}