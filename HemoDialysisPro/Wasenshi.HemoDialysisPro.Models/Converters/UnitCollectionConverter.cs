using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class UnitCollectionConverter : JsonConverter<ICollection<UserUnit>>
    {
        public override ICollection<UserUnit> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<UserUnit>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                int value = reader.GetInt32();
                list.Add(new UserUnit { UnitId = value });
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<UserUnit> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item.UnitId);
            }
            writer.WriteEndArray();
        }
    }
}