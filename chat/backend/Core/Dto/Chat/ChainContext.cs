namespace Chat.Core.Dto.Chat;

public class ChainContext
{
  public record struct Role (
    int Id,
    string Name,
    IEnumerable<string> permission
  );

  public IEnumerable<Role> Roles { get; set; } = Array.Empty<Role>();
  // TODO: something else
}