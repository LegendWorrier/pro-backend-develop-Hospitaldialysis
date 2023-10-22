using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Anticoagulant : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
    }
}
