using System;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IAdmissionService : IApplicationService
    {
        Page<Admission> GetAllAdmissions(int page = 1, int limit = 25, Action<IOrderer<Admission>> orderBy = null, Expression<Func<Admission, bool>> whereCondition = null);
        Page<Admission> GetAdmissionForPatient(string patientId, int page = 1, int limit = 25);

        Admission FindAdmission(Expression<Func<Admission, bool>> expression);
        Admission GetAdmission(Guid id);
        Admission CreateNewAdmission(Admission admission);
        bool UpdateAdmission(Admission admission);
        bool DeleteAdmission(Guid id);
    }
}
