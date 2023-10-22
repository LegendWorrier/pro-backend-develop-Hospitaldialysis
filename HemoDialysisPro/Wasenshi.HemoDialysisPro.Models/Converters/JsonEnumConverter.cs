using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class JsonEnumConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            Type baseType = Nullable.GetUnderlyingType(typeof(T));
            if (baseType != null)
            {
                return (T)Enum.Parse(baseType, value);
            }
            return (T)Enum.Parse(typeof(T), value);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            writer.WriteStringValue(value.ToString());
        }
    }
}
