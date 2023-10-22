using System;

namespace Wasenshi.HemoDialysisPro.ViewModels.Base
{
    public abstract class EntityViewModel
    {
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? Created { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
