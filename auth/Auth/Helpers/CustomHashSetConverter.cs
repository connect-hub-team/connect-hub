using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chat.Auth.Helpers;

public class CustomHashSetConverter<T> : JsonConverter<HashSet<T>>
{
  private readonly Func<string, T> typeFunc;

  public CustomHashSetConverter(Func<string, T> typeFunc)
    => this.typeFunc = typeFunc;

  public override HashSet<T>? Read(ref Utf8JsonReader reader,
    Type typeToConvert, JsonSerializerOptions options)
  {
    if (typeToConvert != typeof(HashSet<T>) ||
        reader.TokenType != JsonTokenType.StartArray)
      return null;

    var set = new HashSet<T>();

    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
      set.Add(typeFunc(reader.GetString() ?? ""));

    return set;
  }

  public override void Write(Utf8JsonWriter writer, HashSet<T> value, JsonSerializerOptions options)
  {
    throw new NotImplementedException();
  }
}