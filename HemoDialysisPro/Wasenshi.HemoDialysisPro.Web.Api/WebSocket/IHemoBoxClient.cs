using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public interface IHemoBoxClient
    {
        Task UnitMetaUpdate(UnitInfo unit, bool remove);

        Task UnitChanged(int unitId); // for root admin only
        Task PatientSelect(PatientInfo patient);
        Task NameChanged(string name);
        Task ChangeState();
        Task Complete();
        // smart update on-going HemoBox
        Task PatientIdChanged(string oldPatientId, string newPatientId);

        // ========== with async response ===========
        Task GetInfo(string correlationId);
        Task GetData(string correlationId);

        Task GetLog(string securekey, int logNo);
    }
}
