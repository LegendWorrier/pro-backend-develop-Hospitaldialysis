using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class BedViewModel : BedBoxInfo
    {
        [JsonIgnore]
        [IgnoreDataMember]
        new public string ConnectionId { get; set; }
    }
}
