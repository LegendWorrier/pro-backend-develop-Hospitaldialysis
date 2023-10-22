using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IShiftInchargeRepository : IRepositoryBase<ShiftIncharge>
    {
        ShiftIncharge Get(int unitId, DateOnly date, bool include = false);
        EntityEntry<ShiftIncharge> AddOrUpdate(ShiftIncharge entity);
        void ClearCollection(ShiftIncharge entity);
    }
}
