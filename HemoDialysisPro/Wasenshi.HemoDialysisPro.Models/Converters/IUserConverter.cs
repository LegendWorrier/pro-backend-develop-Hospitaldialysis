using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models.Converters
{
    public class IUserConverter : IUserConverter<UserBase>
    {
    }

    public class IUserConverter<TUser> : JsonConverter<IUser> where TUser : class, IUser, new()
    {
        // this may be used only on unit test/integration test
        public override IUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = JsonSerializer.Deserialize<TUser>(ref reader, options);
            return result;
        }

        // this is used globally for FE
        public override void Write(Utf8JsonWriter writer, IUser value, JsonSerializerOptions options)
        {
            // use object type to force serialization for all the data in this user object
            JsonSerializer.Serialize<object>(writer, value, options);
        }
    }
}
