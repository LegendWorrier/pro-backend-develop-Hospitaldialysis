namespace Wasenshi.HemoDialysisPro.Models.Stat
{
    public enum StatType
    {
        Avg,
        Count,
        Max,
        Min
    }

    public class StatRowInfo
    {
        public StatType Type { get; set; }
        public object Info { get; set; }
    }
}
