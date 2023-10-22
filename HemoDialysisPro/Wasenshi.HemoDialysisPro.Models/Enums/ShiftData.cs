using System;

namespace Wasenshi.HemoDialysisPro.Models.Enums
{
    [Flags]
    public enum ShiftData
    {
        Undefinded = 0,
        Section1 = 1 << 0,
        Section2 = 1 << 1,
        Section3 = 1 << 2,
        Section4 = 1 << 3,
        Section5 = 1 << 4,
        Section6 = 1 << 5,
        Reserved = 1 << 6,
        OffLimit = 1 << 7
    }
}
