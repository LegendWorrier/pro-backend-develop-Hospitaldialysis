using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditAVShuntViewModel : AVShuntViewModel
    {
        //TODO: create image upload view model
        public new IEnumerable<string> Photographs { get; set; }
    }
}