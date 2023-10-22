using System;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ScheduleMeta : EntityBase<long>
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public TimeOnly? Section1 { get; set; }
        public TimeOnly? Section2 { get; set; }
        public TimeOnly? Section3 { get; set; }
        public TimeOnly? Section4 { get; set; }
        public TimeOnly? Section5 { get; set; }
        public TimeOnly? Section6 { get; set; }
    }
}
