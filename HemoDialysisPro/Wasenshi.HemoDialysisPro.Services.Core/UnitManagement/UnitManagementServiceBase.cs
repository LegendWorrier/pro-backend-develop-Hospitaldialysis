using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.UnitManagement
{
    public abstract class UnitManagementServiceBase
    {
        protected readonly IScheduleUnitOfWork scheduleUOW;
        protected readonly IShiftUnitOfWork shiftUOW;
        protected readonly IMasterDataUOW masterdata;

        public UnitManagementServiceBase(IScheduleUnitOfWork scheduleUow, IShiftUnitOfWork shiftUow, IMasterDataUOW masterdata)
        {
            this.scheduleUOW = scheduleUow;
            this.shiftUOW = shiftUow;
            this.masterdata = masterdata;
        }

        protected ScheduleMeta CreateNewScheduleMeta(int unitId, IEnumerable<ScheduleSection> sections)
        {
            var unit = masterdata.GetMasterRepo<Unit, int>().Find(x => x.Id == unitId).First();
            return CreateNewScheduleMeta(unit, sections);
        }

        protected ScheduleMeta CreateNewScheduleMeta(Unit unit, IEnumerable<ScheduleSection> sections)
        {
            var scheduleMeta = new ScheduleMeta
            {
                IsSystemUpdate = true,
                UnitId = unit.Id,
                UnitName = unit.Name
            };
            UpdateScheduleMeta(scheduleMeta, sections);
            scheduleUOW.ScheduleMeta.Insert(scheduleMeta);
            return scheduleMeta;
        }

        protected void UpdateScheduleMeta(ScheduleMeta scheduleMeta, IEnumerable<ScheduleSection> sections)
        {
            int i = 0;
            foreach (var item in sections)
            {
                switch (i++)
                {
                    case 0:
                        scheduleMeta.Section1 = item.StartTime;
                        break;
                    case 1:
                        scheduleMeta.Section2 = item.StartTime;
                        break;
                    case 2:
                        scheduleMeta.Section3 = item.StartTime;
                        break;
                    case 3:
                        scheduleMeta.Section4 = item.StartTime;
                        break;
                    case 4:
                        scheduleMeta.Section5 = item.StartTime;
                        break;
                    case 5:
                        scheduleMeta.Section6 = item.StartTime;
                        break;
                }
            }
            while (i < 6)
            {
                switch (i++)
                {
                    case 0:
                        scheduleMeta.Section1 = null;
                        break;
                    case 1:
                        scheduleMeta.Section2 = null;
                        break;
                    case 2:
                        scheduleMeta.Section3 = null;
                        break;
                    case 3:
                        scheduleMeta.Section4 = null;
                        break;
                    case 4:
                        scheduleMeta.Section5 = null;
                        break;
                    case 5:
                        scheduleMeta.Section6 = null;
                        break;
                }
            }
        }
    }
}
