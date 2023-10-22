using MessagePack;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class DialysisRecordData : DialysisRecord
    {
        [IgnoreMember]
        new public IEnumerable<DialysisRecordAssessmentItem> AssessmentItems { get; set; }

        public Dictionary<string, AssessmentData> AssessmentDict { get; set; }
    }
}
