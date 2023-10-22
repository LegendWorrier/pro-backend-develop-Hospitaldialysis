using System;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class Common
    {
        public static readonly string PENDING_SECTION_UPDATE = "pending-section-update";
        public static readonly string AUTOFILL = "autofill";

        public static string GetPendingSectionJobId(int unitId) => $"pending-section-jobid-{unitId}";
        public static string GetAutofillJobId(Guid hemoId) => $"{AUTOFILL}-jobid-{hemoId}";
        public static string GetAutoCompleteJobId(Guid hemoId) => $"autocomplete-jobid-{hemoId}";
        public static string GetHemosheetKey(Guid hemoId) => $"urn:hemosheet:{hemoId}";
        public static string GetSessionKey(string patientId) => $"session:{patientId}";

        public static readonly string LAST_AUTOFILL_TIME = "lastAutoFillTime";
        public static readonly string LAST_MACHINE_SAVE_TIME = "lastMachineSaveTime";
        public static readonly string LAST_RECORD_SAVE_TIME = "lastRecordSaveTime";

        public static readonly string SESSION_LIST = "session-list";

        public const string SEE_OWN_PATIENT_ONLY = "see-own-patient-only";
        public const string LOGO_ALIGN = "global:logo-align";
    }
}
