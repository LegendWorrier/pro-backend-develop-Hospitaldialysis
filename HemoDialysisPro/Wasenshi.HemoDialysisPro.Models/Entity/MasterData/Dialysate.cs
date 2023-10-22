using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Dialysate : EntityBase<int>
    {
        [Required]
        public float Ca { get; set; }
        [Required]
        public float K { get; set; }
    }
}
