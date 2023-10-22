using System;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ExecuteViewModel
    {
        public DateTimeOffset? Timestamp { get; set; }
        public ExecutionType? Type { get; set; }
    }
}
