using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameDuMouse.GameMain.Utils
{
    public sealed class SafeEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                if (!string.IsNullOrWhiteSpace(text) && Enum.TryParse<T>(text, true, out var value))
                    return value;

                return default;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numeric))
            {
                var candidate = (T)Enum.ToObject(typeof(T), numeric);
                if (Enum.IsDefined(typeof(T), candidate))
                    return candidate;

                return default;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
