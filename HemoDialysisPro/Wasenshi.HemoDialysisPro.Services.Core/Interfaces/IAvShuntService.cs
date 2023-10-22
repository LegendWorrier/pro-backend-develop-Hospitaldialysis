using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IAvShuntService : IApplicationService
    {
        AVResult GetAvViewResultByPatientId(string patientId);
        IEnumerable<AVShunt> GetAvListByPatientId(string patientId);
        AVShunt GetAvShunt(Guid id);
        AVShunt CreateAvShunt(AVShunt avShunt);
        bool EditAvShunt(AVShunt avShunt);
        bool DeleteAvShunt(Guid id);
        AVShuntIssueTreatment GetIssueTreatment(Guid id);
        AVShuntIssueTreatment CreateIssueTreatment(AVShuntIssueTreatment issueTreatment);
        bool EditIssueTreatment(AVShuntIssueTreatment issueTreatment);
        bool DeleteIssueTreatment(Guid id);
    }
}
