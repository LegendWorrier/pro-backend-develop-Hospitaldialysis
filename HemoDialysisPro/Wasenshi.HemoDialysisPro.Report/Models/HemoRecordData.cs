using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class HemoRecordData
    {
        public PatientData Patient { get; set; }
        public int HdPerWeek { get; set; }
        public string HdExtraStr { get; set; }

        public DateTime Date { get; set; }
        public string DryWeight => Records?.First().DialysisPrescription?.DryWeight + " - " + Records?.Last().DialysisPrescription?.DryWeight;

        public List<HemosheetInfo> Records { get; set; } = new List<HemosheetInfo>();
    }

    public class HemosheetInfo : HemosheetData
    {

        public HemosheetInfo()
        {
            Dehydration = new DehydrationData(this);
        }

        [IgnoreMember]
        new public PatientData Patient { get; set; }

        public HemoNote Note { get; set; }

        public float? KtV { get; set; }
        public float? URR { get; set; }
    }
}
