namespace Wasenshi.HemoDialysisPro.Models.Settings
{
    public class GlobalSetting
    {
        public Align LogoAlign { get; set; }
        public ShiftHistorySetting ShiftHistory { get; set; }
        public HemosheetSetting Hemosheet { get; set; }
        public ScheduleSetting Schedule { get; set; }
        public PatientSetting Patient { get; set; }
    }

    public class ScheduleSetting
    {
        public bool AutoSchedule { get; set; }
    }

    public class PatientSetting
    {
        public bool DoctorCanSeeOwnPatientOnly { get; set; }
    }

    public class ShiftHistorySetting
    {
        public bool Enabled { get; set; }
        public string Limit { get; set; }
    }

    public class HemosheetSetting
    {
        public BasicSetting Basic { get; set; }
        public RuleSetting Rules { get; set; }

        public class BasicSetting
        {
            public string Auto { get; set; }
            public string Delay { get; set; }
            public string AutoFillRecord { get; set; }
            public bool AutoFillMedicine { get; set; }
        }

        public class RuleSetting
        {
            public bool ChangeCompleteTimePermissionRequired { get; set; }
            public bool HeadNurseCanApproveDoctorSignature { get; set; }
            public bool ChangePrescriptionSensitive { get; set; }
            public bool DialysisPrescriptionRequireHeadNurse { get; set; }
        }
        
    }

    public enum Align
    {
        Left,
        Center,
        Right
    }
}
