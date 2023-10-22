using System;

namespace Wasenshi.HemoDialysisPro.Share.GlobalEvents
{
    /// <summary>
    /// Trigger when new hemosheet has been created (new session for a patient).
    /// </summary>
    public class NewHemosheet
    {
        public Guid HemoId { get; set; }
    }
}
