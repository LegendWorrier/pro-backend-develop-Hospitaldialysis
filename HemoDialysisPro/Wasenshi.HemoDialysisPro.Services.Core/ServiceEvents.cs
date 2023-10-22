using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services
{
    /// <summary>
    /// Local events in this instance of hemo server.
    /// </summary>
    public static class ServiceEvents
    {
        public delegate void HemosheetEvent(HemodialysisRecord hemosheet);
        public static event HemosheetEvent OnHemosheetCreated;
        public static event HemosheetEvent OnHemosheetCompleted;

        public delegate void DialysisRecordEvent(DialysisRecord record);
        public static event DialysisRecordEvent OnDialysisRecordCreated;

        public delegate void PatientIdUpdate(string oldId, string newId);
        public static event PatientIdUpdate OnPatientIdUpdated;

        internal static void DispatchCreate(HemodialysisRecord hemodialysisRecord)
        {
            OnHemosheetCreated?.Invoke(hemodialysisRecord);
        }

        internal static void DispatchComplete(HemodialysisRecord hemodialysisRecord)
        {
            OnHemosheetCompleted?.Invoke(hemodialysisRecord);
        }

        internal static void DispatchCreate(DialysisRecord dialysisRecord)
        {
            OnDialysisRecordCreated?.Invoke(dialysisRecord);
        }

        internal static void DispatchPatientIdUpdate(string oldId, string newId)
        {
            OnPatientIdUpdated?.Invoke(oldId, newId);
        }
    }
}
