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

        public static NSDate ToNSDate(this DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }

        public static NSDateComponents ToNSDateComponents(this DateTime date) {

            return new NSDateComponents()
            {
                Year = date.Year,
                Minute = date.Minute,
                Second = date.Second,
                Hour = date.Hour,
                Month = date.Month,
                Day = date.Day
            };
                   
          
        }
    }
}
