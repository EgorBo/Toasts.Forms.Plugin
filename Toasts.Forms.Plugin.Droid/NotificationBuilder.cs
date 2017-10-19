using Android.Graphics;
using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Plugin.Toasts.Interfaces;

namespace Plugin.Toasts
{
    class NotificationBuilder
    {
        public static string PackageName { get; set; }

        public static IDictionary<string, ManualResetEvent> ResetEvent = new Dictionary<string, ManualResetEvent>();
        public static IDictionary<string, NotificationResult> EventResult = new Dictionary<string, NotificationResult>();
        public static List<string> Channels = new List<string>();

        public const string NotificationId = "NOTIFICATION_ID";
        public const string DismissedClickIntent = "android.intent.action.DISMISSED";
        public const string OnClickIntent = "android.intent.action.CLICK";
        public const string NotificationForceOpenApp = "NOTIFICATION_FORCEOPEN";

        public const string DefaultChannelName = "default";

        public const int StartId = 123;

        int _count = 0;
        object _lock = new object();
        static NotificationReceiver _receiver;
        IPlatformOptions _androidOptions;

        public void Init(IPlatformOptions androidOptions)
        {
            PackageName = Application.Context.PackageName;

            IntentFilter filter = new IntentFilter();
            filter.AddAction(DismissedClickIntent);
            filter.AddAction(OnClickIntent);

            _receiver = new NotificationReceiver();

            Application.Context.RegisterReceiver(_receiver, filter);

            _androidOptions = androidOptions;
        }

        public static string GetOrCreateChannel(IAndroidChannelOptions channelOptions)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channelId = channelOptions.Name.Replace(" ", string.Empty).ToLower();
                if (!Channels.Contains(channelId))
                {
                    // Create new channel.
                    var newChannel = new NotificationChannel(channelId, channelOptions.Name, NotificationImportance.High);
                    newChannel.EnableVibration(channelOptions.EnableVibration);
                    newChannel.SetShowBadge(channelOptions.ShowBadge);
                    if (!string.IsNullOrEmpty(channelOptions.Description))
                    {
                        newChannel.Description = channelOptions.Description;
                    }

                    // Register channel.
                    var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
                    notificationManager.CreateNotificationChannel(newChannel);

                    // Save Id for reference.
                    Channels.Add(channelId);
                }
                return channelId;
            }
            return null;
        }

        public IList<INotification> GetDeliveredNotifications()
        {
            IList<INotification> list = new List<INotification>();
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
            {
                return new List<INotification>();
            }

            NotificationManager notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

            foreach (var notification in notificationManager.GetActiveNotifications())
            {
                list.Add(new Notification()
                {
                    Id = notification.Id.ToString(),
                    Title = notification.Notification.Extras.GetString("android.title"),
                    Description = notification.Notification.Extras.GetString("android.text"),
                    Delivered = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(notification.Notification.When)
                });
            }
            return list;
        }

        void ScheduleNotification(string id, INotificationOptions options)
        {
            if (!string.IsNullOrEmpty(id) && options != null)
            {
                var intent = new Intent(Application.Context, typeof(AlarmHandler)).SetAction("AlarmHandlerIntent" + id);

                if (!string.IsNullOrEmpty(options.AndroidOptions.HexColour) && options.AndroidOptions.HexColour.Substring(0, 1) != "#")
                {
                    options.AndroidOptions.HexColour = "#" + options.AndroidOptions.HexColour;
                }

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
                intent.PutExtra(NotificationForceOpenApp, options.AndroidOptions.ForceOpenAppOnNotificationTap);

                var pendingIntent = PendingIntent.GetBroadcast(Application.Context, (StartId + int.Parse(id)), intent, PendingIntentFlags.CancelCurrent);
                var timeTriggered = ConvertToMilliseconds(options.DelayUntil.Value);
                var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;

                alarmManager.Set(AlarmType.RtcWakeup, timeTriggered, pendingIntent);
            }
        }

        long ConvertToMilliseconds(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDifference = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            return utcTime.AddSeconds(-epochDifference).Ticks / 10000;
        }

        string Serialize(ScheduledNotification options)
        {
            var xmlSerializer = new XmlSerializer(typeof(ScheduledNotification));
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, options);
                return stringWriter.ToString();
            }
        }

        public INotificationResult Notify(Activity activity, INotificationOptions options)
        {
            INotificationResult notificationResult = null;
            if (options != null)
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
                    return new NotificationResult() { Action = NotificationAction.NotApplicable, Id = notificationId };
                }

                // Show Notification Right Now
                var dismissIntent = new Intent(DismissedClickIntent);
                dismissIntent.PutExtra(NotificationId, notificationId);

                var pendingDismissIntent = PendingIntent.GetBroadcast(Application.Context, (StartId + notificationId), dismissIntent, 0);

                var clickIntent = new Intent(OnClickIntent);
                clickIntent.PutExtra(NotificationId, notificationId);
                clickIntent.PutExtra(NotificationForceOpenApp, options.AndroidOptions.ForceOpenAppOnNotificationTap);

                // Add custom args
                if (options.CustomArgs != null)
                    foreach (var arg in options.CustomArgs)
                        clickIntent.PutExtra(arg.Key, arg.Value);

                var pendingClickIntent = PendingIntent.GetBroadcast(Application.Context, (StartId + notificationId), clickIntent, 0);

                if (!string.IsNullOrEmpty(options.AndroidOptions.HexColour) && options.AndroidOptions.HexColour.Substring(0, 1) != "#")
                {
                    options.AndroidOptions.HexColour = "#" + options.AndroidOptions.HexColour;
                }

                Android.App.Notification.Builder builder = new Android.App.Notification.Builder(Application.Context)
                    .SetContentTitle(options.AndroidOptions.DebugShowIdInTitle ? "[" + id + "] " + options.Title : options.Title)
                    .SetContentText(options.Description)
                    .SetSmallIcon(smallIcon) // Must have small icon to display
                    .SetPriority((int)NotificationPriority.High) // Must be set to High to get Heads-up notification
                    .SetDefaults(NotificationDefaults.All) // Must also include vibrate to get Heads-up notification
                    .SetAutoCancel(true) // To allow click event to trigger delete Intent
                    .SetContentIntent(pendingClickIntent) // Must have Intent to accept the click                   
                    .SetDeleteIntent(pendingDismissIntent)
                    .SetColor(Color.ParseColor(options.AndroidOptions.HexColour));

                // Notification Channel
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    var notificationChannelId = GetOrCreateChannel(options.AndroidOptions.ChannelOptions);
                    if (!string.IsNullOrEmpty(notificationChannelId))
                    {
                        builder.SetChannelId(notificationChannelId);
                    }
                }

                Android.App.Notification notification = builder.Build();

                NotificationManager notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

                notificationManager.Notify(notificationId, notification);

                if (options.DelayUntil.HasValue)
                {
                    return new NotificationResult() { Action = NotificationAction.NotApplicable, Id = notificationId };
                }

                var timer = new Timer(x => TimerFinished(id, options.ClearFromHistory), null, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(100));

                var resetEvent = new ManualResetEvent(false);
                ResetEvent.Add(id, resetEvent);

                resetEvent.WaitOne(); // Wait for a result

                notificationResult = EventResult[id];

                if (!options.IsClickable && notificationResult.Action == NotificationAction.Clicked)
                {
                    notificationResult.Action = NotificationAction.Dismissed;
                    notificationResult.Id = notificationId;
                }

                if (EventResult.ContainsKey(id))
                {
                    EventResult.Remove(id);
                }
                if (ResetEvent.ContainsKey(id))
                {
                    ResetEvent.Remove(id);
                }

                // Dispose of Intents and Timer
                pendingClickIntent.Cancel();
                pendingDismissIntent.Cancel();
                timer.Dispose();

            }
            return notificationResult;
        }

        public void CancelAll()
        {
            using (NotificationManager notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager)
            {
                notificationManager.CancelAll();
            }
        }

        void TimerFinished(string id, bool cancel)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (cancel) // Will clear from Notification Center
                {
                    using (NotificationManager notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager)
                    {
                        notificationManager.Cancel(Convert.ToInt32(id));
                    }

                    if (ResetEvent.ContainsKey(id))
                    {
                        if (EventResult != null)
                        {
                            EventResult.Add(id, new NotificationResult() { Action = NotificationAction.Timeout, Id = int.Parse(id) });
                        }
                        if (ResetEvent != null && ResetEvent.ContainsKey(id))
                        {
                            ResetEvent[id].Set();
                        }
                    }
                }
            }
        }

    }

    class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var notificationId = intent.Extras.GetInt(NotificationBuilder.NotificationId, -1);
            if (notificationId > -1)
            {
                switch (intent.Action)
                {
                    case NotificationBuilder.OnClickIntent:

                        try
                        {
                            // Attempt to re-focus/open the app.
                            var doForceOpen = intent.Extras.GetBoolean(NotificationBuilder.NotificationForceOpenApp, false);
                            if (doForceOpen)
                            {
                                var packageManager = Application.Context.PackageManager;
                                Intent launchIntent = packageManager.GetLaunchIntentForPackage(NotificationBuilder.PackageName);
                                if (launchIntent != null)
                                {
                                    launchIntent.AddCategory(Intent.CategoryLauncher);
                                    Application.Context.StartActivity(launchIntent);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to re-focus/launch the app: " + ex);
                        }

                        // Click
                        if (NotificationBuilder.EventResult != null && !NotificationBuilder.EventResult.ContainsKey(notificationId.ToString()))
                        {
                            NotificationBuilder.EventResult.Add(notificationId.ToString(), new NotificationResult() { Action = NotificationAction.Clicked, Id = notificationId });
                        }
                        break;

                    default:

                        // Dismiss/Default
                        if (NotificationBuilder.EventResult != null && !NotificationBuilder.EventResult.ContainsKey(notificationId.ToString()))
                        {
                            NotificationBuilder.EventResult.Add(notificationId.ToString(), new NotificationResult() { Action = NotificationAction.Dismissed, Id = notificationId });
                        }
                        break;
                }

                if (NotificationBuilder.ResetEvent != null && NotificationBuilder.ResetEvent.ContainsKey(notificationId.ToString()))
                {
                    NotificationBuilder.ResetEvent[notificationId.ToString()].Set();
                }
            }
        }
    }

}