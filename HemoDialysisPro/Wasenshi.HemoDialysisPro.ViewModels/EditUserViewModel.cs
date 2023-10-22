using System.Text.Json.Serialization;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditUserViewModel : RegisterViewModel
    {
        [PermissionRestrict(Permissions.USER, Permissions.User.EDIT)]
        public new string UserName { get; set; }

        [JsonIgnore]
        public new string Role { get; set; }
    }
}