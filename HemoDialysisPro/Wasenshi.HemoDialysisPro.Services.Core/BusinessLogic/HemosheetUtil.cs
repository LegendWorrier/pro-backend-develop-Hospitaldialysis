using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic
{
    public static class HemosheetUtil
    {
        public static float PreWeight(this DehydrationRecord dehydration) => dehydration.PreTotalWeight > 0 ? dehydration.PreTotalWeight - dehydration.WheelchairWeight - dehydration.ClothWeight : 0;
        public static float FoodIntakeWeight(this DehydrationRecord dehydration) => dehydration.FoodDrinkWeight; // kg
        public static float ExtraFluidTotalWeight(this DehydrationRecord dehydration, DialysisPrescriptionData prescription = null) =>
            dehydration.FoodDrinkWeight * 1000 + (dehydration.BloodTransfusion ?? prescription?.BloodTransfusion ?? 0) + (dehydration.ExtraFluid ?? prescription?.ExtraFluid ?? 0); // ml
        public static float PostWeight(this DehydrationRecord dehydration) => dehydration.PostTotalWeight > 0 ? dehydration.PostTotalWeight - dehydration.PostWheelchairWeight - dehydration.ClothWeight : 0;
    }
}
