using System;

namespace Wasenshi.HemoDialysisPro.Share
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string[] Action { get; set; } = new string[0];

        public NotificationType Type { get; set; } = NotificationType.Info;

        public string[] Tags { get; set; }

        public DateTimeOffset ExpireDate { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        ActionRequired
    }

    public static class NotificationExtensions
    {
        // ============= Converter Util ====================================
        public static double ToSeconds(this DateTime dateTime) => (dateTime - DateTime.MinValue).TotalSeconds;
        public static double ToSeconds(this DateTimeOffset dateTime) => (dateTime - DateTimeOffset.MinValue).TotalSeconds;

        public static DateTime FromSecondToDate(this double seconds) => DateTime.MinValue.AddSeconds(seconds);
    }
}
