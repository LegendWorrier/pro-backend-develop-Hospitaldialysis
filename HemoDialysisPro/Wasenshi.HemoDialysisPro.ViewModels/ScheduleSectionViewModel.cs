using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ScheduleSectionViewModel : EntityViewModel
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int StartTime { get; set; }
    }
}
