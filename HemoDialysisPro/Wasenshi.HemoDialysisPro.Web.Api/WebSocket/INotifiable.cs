using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public interface INotifiable
    {
        Task Notify(NotificationInfo notification);
    }

    public struct NotificationInfo
    {
        public Notification Notification { get; set; }
        public Dictionary<string, string> Languages { get; set; }
    }
}
