using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.PluginBase
{
    /// <summary>
    /// Let you communicate with the hemopro system and command it for specific tasks.
    /// </summary>
    public interface IHemoPro
    {
        Task<byte[]> GenerateHemosheetPdf(Guid hemosheetId);
        Task<byte[]> GenerateHemoAdequacyPdf(string patientId, DateOnly month);

        /// <summary>
        /// This should always be called after the hemosheet pdf has been sent successfully.
        /// </summary>
        /// <param name="hemosheetId"></param>
        /// <returns></returns>
        Task MarkHemosheetAsSent(Guid hemosheetId);

        IConfiguration Configuration { get; }
        TimeZoneInfo TimeZone { get; }

        Patient GetPatient(string patientId);
        Unit GetUnit(int unitId);

        T Resolve<T>();

    }
}
