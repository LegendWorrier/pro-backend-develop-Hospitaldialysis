namespace Wasenshi.HemoDialysisPro.Models.Stat
{
    public class StatInfo
    {
        public string Text { get; set; } // custom stat field (e.g. name of something)
        public int? Count { get; set; } // count value
        public float? Avg { get; set; } // average
        public float? Max { get; set; }
        public float? Min { get; set; }
        public int? TotalCount { get; set; }

        public float? Percent { get; set; }
    }
}
