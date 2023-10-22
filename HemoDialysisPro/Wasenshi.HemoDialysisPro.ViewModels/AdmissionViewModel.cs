using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AdmissionViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string AN { get; set; }
        public DateTimeOffset Admit { get; set; }
        public DateTimeOffset? Discharged { get; set; }

        public ICollection<MasterDataViewModel> Underlying { get; set; } // โรคประจำตัว
        public string ChiefComplaint { get; set; } // อาการสำคัญ (ที่มา admit)
        public string Diagnosis { get; set; }

        public string Room { get; set; } // ตึกที่พักอยู่
        public string TelNo { get; set; } // เบอร์ติดต่อของตึก

        public string StatusDc { get; set; } // status ตอน discharged
        public string TransferTo { get; set; }
    }
}
