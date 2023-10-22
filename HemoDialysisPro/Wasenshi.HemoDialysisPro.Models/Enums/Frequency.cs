namespace Wasenshi.HemoDialysisPro.Models.Enums
{
    public enum Frequency
    {
        /// <summary>
        /// Once a day
        /// </summary>
        QD, // Once a day
        /// <summary>
        /// Once every night
        /// </summary>
        QN, // Once every night
        /// <summary>
        /// Two times a day
        /// </summary>
        BID, // Two times a day
        /// <summary>
        /// Three times a day
        /// </summary>
        TID, // Three times a day
        /// <summary>
        /// Four times a day
        /// </summary>
        QID, // Four times a day
        /// <summary>
        /// Once a week
        /// </summary>
        QW = -7, // Once a week
        /// <summary>
        /// Two times a week
        /// </summary>
        BIW = -6, // Two times a week
        /// <summary>
        /// Three times a week
        /// </summary>
        TIW = -5, // Three times a week
        /// <summary>
        /// Four times a week
        /// </summary>
        QIW = -4, // Four times a week
        /// <summary>
        /// Once every other day
        /// </summary>
        QOD = -2, // Once every other day
        /// <summary>
        /// Once every 2 weeks
        /// </summary>
        Q2W = -14, // Once every 2 weeks
        /// <summary>
        /// Once every 3 weeks
        /// </summary>
        Q3W = -21, // Once every 3 weeks
        /// <summary>
        /// Once every 4 weeks
        /// </summary>
        Q4W = -28, // Once every 4 weeks

        /// <summary>
        /// Use when needed
        /// </summary>
        PRN = 99, // Use when needed
        /// <summary>
        /// Use immediately
        /// </summary>
        ST = -99 // Use immediately
    }
}