using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IAdmissionRepository : IRepository<Admission, Guid>
    {
        IQueryable<AdmissionUnderlying> Underlyings { get; }
    }
}
