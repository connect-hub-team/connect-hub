namespace Chat.Auth.Helpers;

public static class StringToSnakeCase
{
  public static string ToSnakeCase(this string str)
    => string.Concat(str.Select((x, i) 
      => i > 0 && char.IsUpper(x) 
        ? "_" + x
        : x.ToString())).ToLower();
}