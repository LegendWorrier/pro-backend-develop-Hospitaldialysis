using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IMedicinePrescriptionService : IApplicationService
    {
        IEnumerable<MedicinePrescription> GetMedicinePrescriptionByPatientId(string patientId);
        IEnumerable<Guid> GetMedicinePrescriptionAutoList(string patientId, TimeZoneInfo timeZone = null);
        MedicinePrescription GetMedicinePrescription(Guid id);
        MedicinePrescription CreateMedicinePrescription(MedicinePrescription record);
        bool UpdateMedicinePrescription(MedicinePrescription prescription);
        bool DeleteMedicinePrescription(Guid id);
    }
}
