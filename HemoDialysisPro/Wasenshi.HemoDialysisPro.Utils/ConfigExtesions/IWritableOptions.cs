using System;

namespace Wasenshi.HemoDialysisPro.Models.ConfigExtesions
{
    public interface IWritableOptions<T> where T : class, new()
    {
        T Value { get; }
        T Get(string name);
        T GetOrDefault(string name);
        void Update(Action<T> applyChanges);
        void Update(string name, Action<T> applyChanges);
    }
}