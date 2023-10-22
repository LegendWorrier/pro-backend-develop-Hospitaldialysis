using System;

namespace Wasenshi.HemoDialysisPro.Models.Interfaces
{
    public interface IRole : IEntityBase<Guid>
    {
        string Name { get; set; }
        string NormalizedName { get; set; }
    }
}
