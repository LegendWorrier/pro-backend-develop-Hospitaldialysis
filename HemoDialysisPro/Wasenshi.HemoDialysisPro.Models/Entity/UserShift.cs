using System;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class UserShift : EntityBase<long>
    {
        public DateOnly Month { get; set; }
        public Guid UserId { get; set; }

        public bool Suspended { get; set; }

        [NotMapped]
        public IUser User { get; set; }
    }
}
