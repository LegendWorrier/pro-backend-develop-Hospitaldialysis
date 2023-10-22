namespace Wasenshi.HemoDialysisPro.Models
{
    public class Bed : EntityBase<long>
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public string PreferedPatient { get; set; }
    }
}
