using System.Collections.Generic;
using System.Linq;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class TableResult<T>
    {
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<DataRow<T>> Rows { get; set; }
        public IEnumerable<object> Info { get; set; }

        public static implicit operator TableResult<object>(TableResult<T> table)
        {
            return new()
            {
                Columns = table.Columns,
                Info = table.Info,
                Rows = table.Rows.Cast<DataRow<object>>()
            };
        }
    }

    public class Column
    {
        public string Title { get; set; }
        public object Data { get; set; }
    }

    public class DataRow<T>
    {
        public string Title { get; set; }
        public T[] Data { get; set; }
        public int? InfoRef { get; set; }

        public static implicit operator DataRow<object>(DataRow<T> row)
        {
            return new DataRow<object>
            {
                InfoRef = row.InfoRef,
                Title = row.Title,
                Data = row.Data.Cast<object>().ToArray()
            };
        }
    }
}