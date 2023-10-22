namespace Wasenshi.HemoDialysisPro.Constants
{
    public static class Roles
    {
        public const string PowerAdmin = "SuperAdministrator";
        public const string Admin = "Administrator";
        public const string Doctor = "Doctor";
        public const string HeadNurse = "HeadNurse";
        public const string Nurse = "Nurse";
        public const string PN = "PN"; // Practical Nurse / Nurse Assistant

        public const string DoctorUp = "Administrator, Doctor";
        public const string HeadNurseUp = "Administrator, Doctor, HeadNurse";
        public const string HeadNurseOnly = "Administrator, HeadNurse";
        public const string NursesOnly = "Adminstrator, HeadNurse, Nurse, PN";
        public const string NotPN = "Administrator, Doctor, HeadNurse, Nurse";
        public const string NotDoctor = "HeadNurse, Nurse, PN";
        public const string NotDoctorAndPN = "HeadNurse, Nurse";
        public const string All = "Administrator, Doctor, HeadNurse, Nurse, PN";

        public static readonly string[] AllRoles = new[]
        {
            PowerAdmin,
            Admin,
            Doctor,
            HeadNurse,
            Nurse,
            PN
        };
    }
}
