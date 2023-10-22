using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class UnitViewModel : MasterDataViewModel
    {
        public string Code { get; set; }
        public Guid? HeadNurse { get; set; }
    }
}
