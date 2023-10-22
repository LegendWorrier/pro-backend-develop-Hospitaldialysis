using MessagePack;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class DehydrationData : DehydrationRecord
    {
        [MessagePack.IgnoreMember]
        private readonly HemosheetData owner;

        public DehydrationData(HemosheetData owner)
        {
            this.owner = owner;
        }

        [SerializationConstructor]
        public DehydrationData()
        {
            // for serialization
        }

        // =========== Calculate =================
        public float PreWeight => this.PreWeight();
        public float FoodIntakeWeight => this.FoodIntakeWeight(); // kg
        public float ExtraFluidTotal => this.ExtraFluidTotalWeight(owner.DialysisPrescription); // ml
        public float PostWeight => this.PostWeight();

        public float BloodTransfusionActual => this.BloodTransfusion ?? owner.DialysisPrescription?.BloodTransfusion ?? 0; // ml
        public float ExtraFluidActual => this.ExtraFluid ?? owner.DialysisPrescription?.ExtraFluid ?? 0; //ml

        public float UFNet => (owner.DialysisPrescription?.DryWeight.HasValue ?? false) && PreWeight >= 0 ? (PreWeight - owner.DialysisPrescription.DryWeight.Value) : 0; // L
        public float UFEstimate => UFNet > 0 ? (ExtraFluidTotal * 0.001f) + UFNet : 0; // L

        public float TotalUF => UFGoal > 0 ? UFGoal : UFEstimate; // L
    }
}
