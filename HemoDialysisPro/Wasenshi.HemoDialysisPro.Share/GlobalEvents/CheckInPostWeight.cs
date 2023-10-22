using System;

namespace Wasenshi.HemoDialysisPro.Share.GlobalEvents
{
    public class CheckInPostWeight
    {
        public string PatientId { get; set; }
        public Guid? HemoId { get; set; }
        public string ConnectionId { get; set; }
    }
}
