namespace Plugin.Toasts
{
    using Android.App;
    using Android.Content;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml.Serialization;

    internal class NotificationBuilder
    {
        public static IDictionary<string, ManualResetEvent> ResetEvent = new Dictionary<string, ManualResetEvent>();
        public static IDictionary<string, NotificationResult> EventResult = new Dictionary<string, NotificationResult>();

        public const string NotificationId = "NOTIFICATION_ID";
        public const string DismissedClickIntent = "android.intent.action.DISMISSED";
        public const string OnClickIntent = "android.intent.action.CLICK";

        private int _count = 0;
        private object _lock = new object();
        private static NotificationReceiver _receiver;
        private IPlatformOptions _androidOptions;

        public void Init(Activity activity, IPlatformOptions androidOptions)
        {
            IntentFilter filter = new IntentFilter();
            filter.AddAction(DismissedClickIntent);
            filter.AddAction(OnClickIntent);

            _receiver = new NotificationReceiver();

            activity.RegisterReceiver(_receiver, filter);

            _androidOptions = androidOptions;
        }

        public IList<INotification> GetDeliveredNotifications(Activity activity)
        {
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
                return new List<INotification>();

            IList<INotification> list = new List<INotification>();

            NotificationManager notificationManager =
                activity.GetSystemService(Context.NotificationService) as NotificationManager;

            foreach (var notification in notificationManager.GetActiveNotifications())
                list.Add(new Notification()
                {
                    Id = notification.Id.ToString(),
                    Title = notification.Notification.Extras.GetString("android.title"),
                    Description = notification.Notification.Extras.GetString("android.text"),
                    Delivered = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(notification.Notification.When)
                });

            return list;
        }
        private void ScheduleNotification(string id, INotificationOptions options)
        {
            var intent = new Intent(Application.Context, typeof(AlarmHandler))
                .SetAction("AlarmHandlerIntent" + id);

            var notification = new ScheduledNotification()
            {
                AndroidOptions = (AndroidOptions)options.AndroidOptions,
                ClearFromHistory = options.ClearFromHistory,
                DelayUntil = options.DelayUntil,
                Description = options.Description,
                IsClickable = options.IsClickable,
                Title = options.Title
            };
            
            var serializedNotification = Serialize(notification);
            intent.PutExtra(AlarmHandler.NotificationKey, serializedNotification);
            intent.PutExtra(NotificationId, id);

            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
            var triggerTime = NotifyTimeInMilliseconds(options.DelayUntil.Value);
            var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;

            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);

        }

        private long NotifyTimeInMilliseconds(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDifference = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;

            var utcAlarmTimeInMillis = utcTime.AddSeconds(-epochDifference).Ticks / 10000;
            return utcAlarmTimeInMillis;
        }

        private string Serialize(ScheduledNotification options)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(ScheduledNotification));
                using (var stringWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(stringWriter, options);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public INotificationResult Notify(Activity activity, INotificationOptions options)
        {

            var notificationId = _count;
            var id = _count.ToString();
            _count++;

            int smallIcon;

            if (options.AndroidOptions.SmallDrawableIcon.HasValue)
                smallIcon = options.AndroidOptions.SmallDrawableIcon.Value;
            else if (_androidOptions.SmallIconDrawable.HasValue)
                smallIcon = _androidOptions.SmallIconDrawable.Value;
            else
                smallIcon = Android.Resource.Drawable.IcDialogInfo; // As last resort

            if (options.DelayUntil.HasValue)
            {
                options.AndroidOptions.SmallDrawableIcon = smallIcon;
                ScheduleNotification(id, options);
                return new NotificationResult() { Action = NotificationAction.NotApplicable };
            }

            // Show Notification Right Now
            Intent dismissIntent = new Intent(DismissedClickIntent);
            dismissIntent.PutExtra(NotificationId, notificationId);

            PendingIntent pendingDismissIntent = PendingIntent.GetBroadcast(activity.ApplicationContext, 123, dismissIntent, 0);

            Intent clickIntent = new Intent(OnClickIntent);
            clickIntent.PutExtra(NotificationId, notificationId);

            // Add custom args
            if (options.CustomArgs != null)
                foreach (var arg in options.CustomArgs)
                    clickIntent.PutExtra(arg.Key, arg.Value);

            PendingIntent pendingClickIntent = PendingIntent.GetBroadcast(activity.ApplicationContext, 123, clickIntent, 0);

            Android.App.Notification.Builder builder = new Android.App.Notification.Builder(activity)
                    .SetContentTitle(options.Title)
                    .SetContentText(options.Description)
                    .SetSmallIcon(smallIcon) // Must have small icon to display
                    .SetPriority((int)NotificationPriority.High) // Must be set to High to get Heads-up notification
                    .SetDefaults(NotificationDefaults.All) // Must also include vibrate to get Heads-up notification
                    .SetAutoCancel(true) // To allow click event to trigger delete Intent
                    .SetContentIntent(pendingClickIntent) // Must have Intent to accept the click                   
                    .SetDeleteIntent(pendingDismissIntent);

            Android.App.Notification notification = builder.Build();

            NotificationManager notificationManager =
                activity.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(notificationId, notification);

            if (options.DelayUntil.HasValue)
                return new NotificationResult() { Action = NotificationAction.NotApplicable };

            var timer = new Timer(x => TimerFinished(activity, id, options.ClearFromHistory), null, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(100));

            var resetEvent = new ManualResetEvent(false);
            ResetEvent.Add(id, resetEvent);

            resetEvent.WaitOne(); // Wait for a result

            var notificationResult = EventResult[id];

            if (!options.IsClickable && notificationResult.Action == NotificationAction.Clicked)
                notificationResult.Action = NotificationAction.Dismissed;

            EventResult.Remove(id);
            ResetEvent.Remove(id);

            // Dispose of Intents and Timer
            pendingClickIntent.Cancel();
            pendingDismissIntent.Cancel();
            timer.Dispose();

            return notificationResult;
        }

        public void CancelAll(Activity activity)
        {
            NotificationManager notificationManager =
                activity.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.CancelAll();
        }

        private void TimerFinished(Activity activity, string id, bool cancel)
        {
            if (cancel) // Will clear from Notification Center
            {
                NotificationManager notificationManager =
                   activity.GetSystemService(Context.NotificationService) as NotificationManager;

                notificationManager.Cancel(Convert.ToInt32(id));
            }

            if (ResetEvent.ContainsKey(id))
            {
                EventResult.Add(id, new NotificationResult() { Action = NotificationAction.Timeout });
                ResetEvent[id].Set();
            }
        }

    }

    internal class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var notificationId = intent.Extras.GetInt(NotificationBuilder.NotificationId).ToString();

            switch (intent.Action)
            {
                case NotificationBuilder.OnClickIntent:
                    NotificationBuilder.EventResult.Add(notificationId, new NotificationResult() { Action = NotificationAction.Clicked });
                    break;
                default:
                case NotificationBuilder.DismissedClickIntent:
                    NotificationBuilder.EventResult.Add(notificationId, new NotificationResult() { Action = NotificationAction.Dismissed });
                    break;
            }

            NotificationBuilder.ResetEvent[notificationId].Set();
        }
    }

}