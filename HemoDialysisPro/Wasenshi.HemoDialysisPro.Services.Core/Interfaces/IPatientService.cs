using System;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IPatientService : IApplicationService
    {
        Page<Patient> GetAllPatients(int page = 1, int limit = 25, Action<IOrderer<Patient>> orderBy = null, Expression<Func<Patient, bool>> whereCondition = null);
        Page<Patient> GetUnitPatients(int unitId, int page = 1, int limit = 25, Expression<Func<Patient, bool>> whereCondition = null);
        Page<Patient> GetDoctorPatients(Guid doctorId, int page = 1, int limit = 25, Action<IOrderer<Patient>> orderBy = null, Expression<Func<Patient, bool>> whereCondition = null);
        int CountAll(Expression<Func<Patient, bool>> whereCondition = null);
        Patient FindPatient(Expression<Func<Patient, bool>> expression);
        Patient GetPatient(string id);
        Patient GetPatientByRFID(string rfid);
        Patient CreateNewPatient(Patient patient);
        bool UpdatePatient(Patient patient, string newId = null);
        bool DeletePatient(string id);
        // =============== Patient History ==============================
        IEnumerable<PatientHistory> GetPatientHistory(string patientId);
        bool UpdatePatientHistory(string patientId, IEnumerable<PatientHistory> entries);
    }
}
