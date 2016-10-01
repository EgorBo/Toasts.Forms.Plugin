namespace Plugin.Toasts
{
    using Foundation;
    using System.Collections.Generic;
    using System.Threading;
    using UIKit;

    public class LocalNotificationManager
    {
        private static object _lock = new object();

        public INotificationResult Notify(INotificationOptions options)
        {
            // create the notification
            var notification = new UILocalNotification();

            // set the fire date (the date time in which it will fire)
            notification.FireDate = NSDate.Now;
           
            // configure the alert
            notification.AlertTitle = options.Title;
            notification.AlertBody = options.Description;
            
            // set the sound to be the default sound
            notification.SoundName = UILocalNotification.DefaultSoundName;

            // schedule it
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);

            return new NotificationResult() { Action = NotificationAction.NotApplicable };
        }
    }
}
