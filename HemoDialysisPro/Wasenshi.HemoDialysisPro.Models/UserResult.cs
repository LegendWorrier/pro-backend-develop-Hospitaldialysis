using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Converters;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class UserResult
    {
        [JsonConverter(typeof(IUserConverter))]
        public IUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
