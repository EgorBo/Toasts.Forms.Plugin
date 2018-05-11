using Android.App;
using Android.Content;
using System;
using System.IO;
using System.Xml.Serialization;
using Android.Graphics;
using Android.Support.V4.App;

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

            if (!string.IsNullOrEmpty(options.AndroidOptions.HexColor) && options.AndroidOptions.HexColor.Substring(0, 1) != "#")
            {
                options.AndroidOptions.HexColor = "#" + options.AndroidOptions.HexColor;
            }

            // Show Notification
            var builder = new NotificationCompat.Builder(Application.Context, NotificationBuilder.DefaultChannelName)
                .SetContentTitle(options.Title)
                .SetContentText(options.Description)
                .SetSmallIcon(options.AndroidOptions.SmallDrawableIcon.Value) // Must have small icon to display
                .SetPriority((int)NotificationPriority.High) // Must be set to High to get Heads-up notification
                .SetDefaults((int) NotificationDefaults.All) // Must also include vibrate to get Heads-up notification
                .SetAutoCancel(true)
                .SetColor(Color.ParseColor(options.AndroidOptions.HexColor));

            if (options.AndroidOptions.ForceOpenAppOnNotificationTap)
            {
                var clickIntent = new Intent(NotificationBuilder.OnClickIntent);
                clickIntent.PutExtra(NotificationBuilder.NotificationId, int.Parse(id));
                clickIntent.PutExtra(NotificationBuilder.NotificationForceOpenApp, options.AndroidOptions.ForceOpenAppOnNotificationTap);
                var pendingClickIntent = PendingIntent.GetBroadcast(Application.Context, (NotificationBuilder.StartId + int.Parse(id)), clickIntent, 0);
                builder.SetContentIntent(pendingClickIntent);
            }

            // Notification Channel
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var notificationChannelId = NotificationBuilder.GetOrCreateChannel(options.AndroidOptions.ChannelOptions);
                if (!string.IsNullOrEmpty(notificationChannelId))
                {
                    builder.SetChannelId(notificationChannelId);
                }
            }

            Android.App.Notification notification = builder.Build();

            NotificationManager notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(Convert.ToInt32(id), notification);
        }

        private ScheduledNotification DeserializeNotification(string notificationString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ScheduledNotification));
            using (var stringReader = new StringReader(notificationString))
            {
                return (ScheduledNotification)xmlSerializer.Deserialize(stringReader);
            }
        }
    }
}