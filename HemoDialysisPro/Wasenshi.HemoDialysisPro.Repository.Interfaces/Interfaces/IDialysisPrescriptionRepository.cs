using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IDialysisPrescriptionRepository : IRepository<DialysisPrescription, Guid>
    {
    }
}
