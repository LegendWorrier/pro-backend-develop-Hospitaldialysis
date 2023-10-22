using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class DeathCause : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
    }
}