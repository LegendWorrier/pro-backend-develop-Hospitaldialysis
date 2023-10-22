using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.ViewModels.Users
{
    public class UserPwdHashHidden : User
    {
        [JsonIgnore]
        new public string PasswordHash { get; set; }
    }
}
