using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class TableResultViewModel<T>
    {
        public IEnumerable<ColumnViewModel> Columns { get; set; }
        public IEnumerable<DataRowViewModel<T>> Rows { get; set; }
        public IEnumerable<object> Info { get; set; }
    }

    public class ColumnViewModel
    {
        public string Title { get; set; }
        public object Data { get; set; }
    }

    public class DataRowViewModel<T>
    {
        public string Title { get; set; }
        public T[] Data { get; set; }
        public int? InfoRef { get; set; }
    }
}
