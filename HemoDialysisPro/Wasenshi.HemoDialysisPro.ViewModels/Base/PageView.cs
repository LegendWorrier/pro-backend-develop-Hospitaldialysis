using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels.Base
{
    public class PageView<T> where T : class
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
    }
}
