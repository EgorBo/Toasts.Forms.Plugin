using Android.App;
using Android.Content;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Plugin.Toasts
{
    [BroadcastReceiver(Enabled = true, Label = "Toasts Broadcast Receiver")]
    public class AlarmHandler : BroadcastReceiver
    {
        public const string NotificationKey = "LocalNotification";
           
        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(NotificationKey);
            var id = intent.GetStringExtra(NotificationBuilder.NotificationId);
            var options = DeserializeNotification(extra);

            // Show Notification
            Android.App.Notification.Builder builder = new Android.App.Notification.Builder(Application.Context)
                     .SetContentTitle(options.Title)
                     .SetContentText(options.Description)
                     .SetSmallIcon(options.AndroidOptions.SmallDrawableIcon.Value) // Must have small icon to display
                     .SetPriority((int)NotificationPriority.High) // Must be set to High to get Heads-up notification
                     .SetDefaults(NotificationDefaults.All) // Must also include vibrate to get Heads-up notification
                     .SetAutoCancel(true);

            Android.App.Notification notification = builder.Build();

            NotificationManager notificationManager =
                Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(Convert.ToInt32(id), notification);
        }

        private ScheduledNotification DeserializeNotification(string notificationString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ScheduledNotification));
            using (var stringReader = new StringReader(notificationString))
                return (ScheduledNotification)xmlSerializer.Deserialize(stringReader);                
        }
    }
}