﻿using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Underlying : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
    }
}