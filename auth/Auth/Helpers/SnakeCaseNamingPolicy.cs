using System.Text.Json;

namespace Chat.Auth.Helpers;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
  public static SnakeCaseNamingPolicy Instance { get; } = new();

  public override string ConvertName(string name)
    => name.ToSnakeCase();
}