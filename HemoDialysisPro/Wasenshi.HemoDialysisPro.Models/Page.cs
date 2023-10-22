using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Page<T> where T : class
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
    }
}
