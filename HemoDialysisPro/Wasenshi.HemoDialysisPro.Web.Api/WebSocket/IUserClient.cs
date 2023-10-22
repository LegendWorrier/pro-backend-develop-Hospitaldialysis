using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public interface IUserClient : INotifiable
    {
        Task BedUpdate(BedViewModel bed);
        Task BedDelete(string macAddress);
        Task BedPatient(string macAddress, PatientInfo patient);
        Task BoxStatus(string macAddress, BoxStatus status);
        Task BoxAlert(string macAddress, Alarm alarm);
        Task BoxChangeUnit(string macAddress, int unitId);
    }
}
