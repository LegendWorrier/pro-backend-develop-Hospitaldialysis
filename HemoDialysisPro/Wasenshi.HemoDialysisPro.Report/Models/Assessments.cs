using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class Assessments
    {
        public Dictionary<string, IMetaData> Metadata { get; set; } = new Dictionary<string, IMetaData>();
        public Dictionary<string, IMetaData> OptionMetadata { get; set; } = new Dictionary<string, IMetaData>();

        public Dictionary<string, AssessmentData> Pre { get; set; } = new Dictionary<string, AssessmentData>();
        public Dictionary<string, AssessmentData> Re { get; set; } = new Dictionary<string, AssessmentData>();
        public Dictionary<string, AssessmentData> Post { get; set; } = new Dictionary<string, AssessmentData>();
        public Dictionary<string, AssessmentData> Other { get; set; } = new Dictionary<string, AssessmentData>();
    }
}