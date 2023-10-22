namespace Wasenshi.HemoDialysisPro.Share
{
    public static class Permissions
    {
        public const string GLOBAL = "global"; // this permission is for superadmin only (rootadmin)
        public static class MasterData
        {
            public const string GENERAL = "masterdata";

            public const string UNIT = "unit";

            public const string DIALYZER = "dialyzer";
            public const string MEDICINE = "medicine";
            public const string MED_CATEGORY = "medicine-category";
            public const string MEDSUPPLY = "med-supply";
            public const string EQUIPMENT = "equipment";

            public const string ANTICOAGULANT = "ac";
            public const string DEATHCAUSE = "death-cause";
            public const string DIALYSATE = "dialysate";
            public const string LAB = "lab-item";
            public const string NEEDLE = "needle";
            public const string STATUS = "status";
            public const string UNDERLYING = "underlying";
            public const string WARD = "ward";
            public const string PATIENT_HISTORY = "patient-history";
        }

        public static class User
        {
            public const string ADD = "user-add";
            public const string EDIT = "user-edit";
            public const string DELETE = "user-del";

            public const string ADD_PERMISSION = $"{USER}, {ADD}";
            public const string EDIT_PERMISSION = $"{USER}, {EDIT}";
            public const string DEL_PERMISSION = $"{USER}, {DELETE}";
        }

        public static class Patient
        {
            public const string SETTING = "patient-setting";
            public const string RULE = "patient-rule";
            public const string DELETE = "patient-del";
        }

        public static class Hemosheet
        {
            public const string SETTING = "hemosheet-setting";
            public const string RULE = "hemosheet-rule";
            public const string DELETE = "hemosheet-del";
        }

        public const string BASIC = "basic";
        public const string ASSESSMENT = "assessment";
        public const string USER = "user";
        public const string DIALYSIS = "dialysis-record";
        public const string PRESCRIPTION = "prescription";
        public const string HEMOSHEET = $"{Hemosheet.SETTING}, {Hemosheet.RULE}";
        public const string SCHEDULE = "schedule";
        public const string SHIFTHISTORY = "shift-history";
        public const string LABEXAM = "lab-exam";
        public const string CONFIG = $"config, {BASIC}, {DIALYSIS}, {PRESCRIPTION}, {Patient.SETTING}";

        public const string UNIT_SETTING = "unit-setting";

        public const string STOCK = "stock";

    }

    public readonly struct PermissionInfo
    {
        // ========= Master Data ============
        public static readonly PermissionInfo GENERAL = new(Permissions.MasterData.GENERAL, "Manage all master data (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id, true);
        public static readonly PermissionInfo UNIT = new(Permissions.MasterData.UNIT, "Manage units (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo DIALYZER = new(Permissions.MasterData.DIALYZER, "Manage dialyzers list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo MEDICINE = new(Permissions.MasterData.MEDICINE, "Manage medicines list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo MED_CATEGORY = new(Permissions.MasterData.MED_CATEGORY, "Manage medicine categories (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo MEDSUPPLY = new(Permissions.MasterData.MEDSUPPLY, "Manage medical supplies (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo EQUIPMENT = new(Permissions.MasterData.EQUIPMENT, "Manage equipments (add/edit/delete)",  PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo ANTICOAGULANT = new(Permissions.MasterData.ANTICOAGULANT, "Manage anticoagulant list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo DEATHCAUSE = new(Permissions.MasterData.DEATHCAUSE, "Manage death cause list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo DIALYSATE = new(Permissions.MasterData.DIALYSATE, "Manage dialysate formula list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo LAB = new(Permissions.MasterData.LAB, "Manage lab items list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo NEEDLE = new(Permissions.MasterData.NEEDLE, "Manage needles list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo STATUS = new(Permissions.MasterData.STATUS, "Manage status list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo UNDERLYING = new(Permissions.MasterData.UNDERLYING, "Manage underlying list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo WARD = new(Permissions.MasterData.WARD, "Manage wards list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);
        public static readonly PermissionInfo PATIENT_HISTORY = new(Permissions.MasterData.PATIENT_HISTORY, "Manage patient history entry list (add/edit/delete)", PermissionGroupInfo.MASTERDATA.Id);

        // ================== Users =============
        public static class User
        {
            public static readonly PermissionInfo USER = new(Permissions.USER, "Manage users (add/edit/delete)", PermissionGroupInfo.USER.Id, true);
            public static readonly PermissionInfo ADD = new(Permissions.User.ADD, "Add user", PermissionGroupInfo.USER.Id);
            public static readonly PermissionInfo EDIT = new(Permissions.User.EDIT, "Edit user", PermissionGroupInfo.USER.Id);
            public static readonly PermissionInfo DELETE = new(Permissions.User.DELETE, "Delete user", PermissionGroupInfo.USER.Id);
        }
        public static class Patient
        {
            public static readonly PermissionInfo RULE = new(Permissions.Patient.RULE, "Manage rule about patient/doctor accessibility.");
            public static readonly PermissionInfo DELETE = new(Permissions.Patient.DELETE, "Delete patient");
        }

        public static class Hemosheet
        {
            public static readonly PermissionInfo SETTING = new(Permissions.Hemosheet.SETTING, "Manage hemosheet basic settings", PermissionGroupInfo.HEMOSHEET.Id);
            public static readonly PermissionInfo RULE = new(Permissions.Hemosheet.RULE, "Manage hemosheet rules", PermissionGroupInfo.HEMOSHEET.Id);
            public static readonly PermissionInfo DELETE = new(Permissions.Hemosheet.DELETE, "Delete hemosheet", PermissionGroupInfo.HEMOSHEET.Id);
        }

        // ================ General Setting =============
        public static readonly PermissionInfo BASIC = new(Permissions.BASIC, "Manage hospital/center basic settings");
        public static readonly PermissionInfo ASSESSMENT = new(Permissions.ASSESSMENT, "Manage assessments settings");
        public static readonly PermissionInfo DIALYSIS = new(Permissions.DIALYSIS, "Manage dialysis record settings");
        public static readonly PermissionInfo SCHEDULE = new(Permissions.SCHEDULE, "Manage schedule related settings");
        public static readonly PermissionInfo SHIFTHISTORY = new(Permissions.SHIFTHISTORY, "Manage shift history related settings");
        public static readonly PermissionInfo LABEXAM = new(Permissions.LABEXAM, "Manage lab exam related settings");

        public static readonly PermissionInfo UNIT_SETTING = new(Permissions.UNIT_SETTING, "Manage unit rules and settings (limited to only units assigned for this user)");

        // ================ Finance & Stock Management =========================

        public static readonly PermissionInfo STOCK = new(Permissions.STOCK, "Manage Stocks", PermissionGroupInfo.MANAGEMENT.Id);

        public PermissionInfo(string name, string desc, int? groupId = null, bool forAll = false)
        {
            Name = name;
            Description = desc;
            GroupId = groupId;
            ForAll = forAll;
        }
        public string Name { get; }
        public string Description { get; }
        public int? GroupId { get; }
        public bool ForAll { get; }
    }

    public readonly struct PermissionGroupInfo
    {
        public static readonly PermissionGroupInfo MASTERDATA = new(1, "Master Data", "Manage masterdata");
        public static readonly PermissionGroupInfo USER = new(2, "Users Management", 
            "Manage users. Users are protected by permission level, so admins cannot change each other account, and normal users also cannot change admin account.\n" +
            "Permission level are as followed: root > admin > doctor > head nurse > nurse > PN");
        public static readonly PermissionGroupInfo HEMOSHEET = new(3, "Hemosheet", "Manage hemosheet rule & setting");

        public static readonly PermissionGroupInfo MANAGEMENT = new(4, "Management & Finance", "Manage finance & stocks");

        public PermissionGroupInfo(int id, string name, string desc)
        {
            Id = id;
            Name = name;
            Description = desc;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
