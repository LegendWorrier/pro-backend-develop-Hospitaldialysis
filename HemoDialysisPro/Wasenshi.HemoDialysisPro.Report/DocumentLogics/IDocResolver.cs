using System.Collections.Generic;
using System.Threading.Tasks;
using Telerik.Reporting;

namespace Wasenshi.HemoDialysisPro.Report.DocumentLogics
{
    public interface IDocResolver
    {
        Task<object> PrepareData(IDictionary<string, object> parameters);
        object GetData(object data);
        Task<object> UpdateData(object prevData);

        void ExtraSetup(InstanceReportSource report);
    }
}
