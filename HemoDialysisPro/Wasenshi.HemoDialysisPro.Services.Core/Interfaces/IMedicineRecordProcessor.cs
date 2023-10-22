using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IMedicineRecordProcessor : IApplicationService
    {
        bool CheckAvailablity(MedicinePrescription prescription, out string reason, TimeZoneInfo timeZone = null);
    }
}
