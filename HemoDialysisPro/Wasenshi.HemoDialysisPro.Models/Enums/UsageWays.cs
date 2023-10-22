using System;

namespace Wasenshi.HemoDialysisPro.Models.Enums
{
    [Flags]
    public enum UsageWays
    {
        None = 0,
        PO = 1 << 0, // Oral
        SL = 1 << 1, // Sublingual
        SC = 1 << 2, // Subcutaneous injection
        IV = 1 << 3, // Intravenous injection
        IM = 1 << 4, // Intramuscular injection
        IVD = 1 << 5, // Intravenous addition
        TOPI = 1 << 6, // Partially rubbed
        EXT = 1 << 7, // External use
        AC = 1 << 8, // Take before meals
        PC = 1 << 9, // Take after meals
        Meal = 1 << 10, // Taken in meal
        HOME = 1 << 11, // Patient treat him/herself at home
    }
}
