using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    public abstract class EntityBase<TKey> : EntityBase, IEntityBase<TKey>, IEqualityComparer<IEntityBase<TKey>>
    {
        [Column(Order = 0)]
        [Key]
        public TKey Id { get; set; }

        // =========== Equality Interfaces ==============

        public bool Equals(IEntityBase<TKey> x, IEntityBase<TKey> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode([DisallowNull] IEntityBase<TKey> obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public abstract class EntityBase : IEntityBase
    {
        [Column(Order = 995)]
        public DateTime? Created { get; set; }
        [Column(Order = 996)]
        public Guid CreatedBy { get; set; }
        [Column(Order = 997)]
        public DateTime? Updated { get; set; }
        [Column(Order = 998)]
        public Guid? UpdatedBy { get; set; }

        [Column(Order = 999), DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public bool IsSystemUpdate { get; set; } = false;

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
