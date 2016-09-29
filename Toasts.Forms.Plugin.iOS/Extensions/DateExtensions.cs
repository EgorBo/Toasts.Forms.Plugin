namespace Plugin.Toasts.Extensions
{
    using Foundation;
    using System;

    public static class DateExtensions
    {
        private static DateTime _nsRef = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this NSDate nsDate)
        {
            return _nsRef.AddSeconds(nsDate.SecondsSinceReferenceDate);
        }
    }
}
