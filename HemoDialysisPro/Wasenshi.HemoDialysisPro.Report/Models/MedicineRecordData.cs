using MessagePack;
using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class MedicineRecordData : MedicineRecord
    {
        [MessagePack.IgnoreMember]
        private readonly IUserResolver userResolver;

        public MedicineRecordData(IUserResolver userResolver)
        {
            this.userResolver = userResolver;
        }

        [SerializationConstructor]
        public MedicineRecordData()
        {
            // for serialization
        }

        public string MedicineName => Prescription.Medicine.Name;
        public float Dose => OverrideDose ?? Prescription.OverrideDose ?? Prescription.Medicine.Dose ?? 1f;
        public float TotalAmount => Dose * Prescription.Quantity;
        public string Unit => Prescription.OverrideUnit ?? Prescription.Medicine.PieceUnit ?? (Dose > 1 ? "Pcs" : "Pc");

        public string Route => OverrideRoute.HasValue ? RouteMap[OverrideRoute.Value] : RouteMap[Prescription.Route];

        public string CreatorName => userResolver.GetName(CreatedBy);
        public string CreatorEmployeeId => userResolver.GetEmployeeId(CreatedBy);
        public string CoSignName => userResolver.GetName(CoSign ?? Guid.Empty);
        public string CoSignEmployeeId => userResolver.GetEmployeeId(CoSign ?? Guid.Empty);


        public static readonly Dictionary<UsageWays, string> RouteMap = new Dictionary<UsageWays, string>
        {
            {UsageWays.PO, "PO - Oral"},
            {UsageWays.SL, "SL - Sublingual"},
            {UsageWays.SC, "SC - Subcutaneous injection"},
            {UsageWays.IV, "IV - Intravenous injection"},
            {UsageWays.IM, "IM - Intramuscular injection"},
            {UsageWays.IVD, "IVD - Intravenous addition"},
            {UsageWays.TOPI, "TOPI - Partially rubbed"},
            {UsageWays.EXT, "EXT - External use"},
            {UsageWays.AC, "AC - Take before meals"},
            {UsageWays.PC, "PC - Take after meals"},
            {UsageWays.Meal, "Meal - Taken in meal"},
            {UsageWays.HOME, "Home (Self-Treat)"},
        };
    }
}
