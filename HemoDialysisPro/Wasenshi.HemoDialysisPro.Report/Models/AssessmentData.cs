using System.Linq;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class AssessmentData : AssessmentItem
    {
        public new IMetaData[] Selected { get; set; }

        public string[] SelectedAsStrings => Selected.Select(x => x.DisplayName).ToArray();
    }
}
