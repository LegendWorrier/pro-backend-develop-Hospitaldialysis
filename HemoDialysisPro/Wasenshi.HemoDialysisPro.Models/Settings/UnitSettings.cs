namespace Wasenshi.HemoDialysisPro.Models.Settings
{
    public class UnitSettings
    {
        public int? MaxPatientPerSlot { get; set; }

        public bool? AutoNurseInShift { get; set; }
        public bool? AutoSendHemosheetWhenFinish { get; set; }
    }
}
