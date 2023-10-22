using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IScheduleProcessor : IApplicationService
    {
        ScheduleResult ProcessSlotData(IEnumerable<ScheduleSection> sections, IEnumerable<SectionSlotPatient> slots);
    }
}
