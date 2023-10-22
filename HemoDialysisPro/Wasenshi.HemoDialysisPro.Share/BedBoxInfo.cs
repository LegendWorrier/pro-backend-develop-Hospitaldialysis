namespace Wasenshi.HemoDialysisPro.Share
{
    public class BedBoxInfo
    {
        public PatientInfo Patient { get; set; }
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public int? UnitId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public string PreferedPatient { get; set; }

        // ======== Non-Persistant states ================
        public string ConnectionId { get; set; } // Important for real-time feedback and connection
        public string PatientId { get; set; }

        public bool Online { get; set; } = false;
        public bool Sending { get; set; }
        public bool IsRegistered { get; set; } = true;
    }
}
