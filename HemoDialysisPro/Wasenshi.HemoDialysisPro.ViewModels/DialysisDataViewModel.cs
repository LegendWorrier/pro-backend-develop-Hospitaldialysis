using System;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class DialysisData : DialysisRecordViewModel
    {
        public string MacAddress { get; set; } // ignored in websocket protocol
        public string PatientId { get; set; } // ignored in websocket protocol

        public int? TotalTime { get; set; }

        [JsonIgnore]
        public new Guid Id { get; set; }
        [JsonIgnore]
        public new Guid HemodialysisId { get; set; }

        [JsonIgnore]
        public new string Note { get; set; }
    }
}
