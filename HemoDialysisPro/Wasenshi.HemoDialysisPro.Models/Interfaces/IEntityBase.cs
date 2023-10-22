using System;

namespace Wasenshi.HemoDialysisPro.Models.Interfaces
{
    public interface IEntityBase<TKey> : IEntityBase
    {
        TKey Id { get; set; }
    }

    public interface IEntityBase : ICloneable
    {
    }
}
