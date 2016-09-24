namespace Plugin.Toasts
{
    using Android.App;
    using Android.Content;
    using System.Collections.Generic;
    using System.Threading;

    public class NotificationBuilder
    {
        public static IDictionary<string, ManualResetEvent> ResetEvent = new Dictionary<string, ManualResetEvent>();
        public static IDictionary<string, NotificationResult> EventResult = new Dictionary<string, NotificationResult>();
      
        public const string NotificationId = "NOTIFICATION_ID";
        public const string DismissedClickIntent = "android.intent.action.DISMISSED";
        public const string OnClickIntent = "android.intent.action.CLICK";

        private int _count = 0;
        private object _lock = new object();
        private static NotificationReceiver _receiver;

        public void Init(Activity activity)
        {
            IntentFilter filter = new IntentFilter();
            filter.AddAction(DismissedClickIntent);
            filter.AddAction(OnClickIntent);

            _receiver = new NotificationReceiver();

            activity.RegisterReceiver(_receiver, filter);
        }

        public NotificationResult Notify(Activity activity, INotificationOptions options)
        {
            var notificationId = _count;
            var id = _count.ToString();
            _count++;

            Intent dismissIntent = new Intent(DismissedClickIntent);
            dismissIntent.PutExtra(NotificationId, notificationId);          

            PendingIntent pendingDismissIntent = PendingIntent.GetBroadcast(activity.ApplicationContext, 123, dismissIntent, 0);

            Intent clickIntent = new Intent(OnClickIntent);
            clickIntent.PutExtra(NotificationId, notificationId);

            PendingIntent pendingClickIntent = PendingIntent.GetBroadcast(activity.ApplicationContext, 123, clickIntent, 0);

            Notification.Builder builder = new Notification.Builder(activity)
                    .SetContentTitle(options.Title)
                    .SetContentText(options.Description)
                    .SetSmallIcon(Android.Resource.Drawable.ButtonStar) //Android.Graphics.Drawables.Icon.CreateWithContentUri
                    .SetPriority((int)NotificationPriority.High)
                    .SetDefaults(NotificationDefaults.All)
                    .SetAutoCancel(true) // To allow click event to trigger delete Intent
                    .SetContentIntent(pendingClickIntent) // Must have Intent to accept the click
                    .SetDeleteIntent(pendingDismissIntent);

            Notification notification = builder.Build();

            NotificationManager notificationManager =
                activity.GetSystemService(Context.NotificationService) as NotificationManager;
            
            notificationManager.Notify(notificationId, notification);

            var resetEvent = new ManualResetEvent(false);
            ResetEvent.Add(id, resetEvent);
            
            resetEvent.WaitOne(); // Wait for a result

            var notificationResult = EventResult[id];

            if (!options.IsClickable && notificationResult == NotificationResult.Clicked)
                notificationResult = NotificationResult.Dismissed;

            EventResult.Remove(id);
            ResetEvent.Remove(id);
            pendingClickIntent.Cancel();
            pendingDismissIntent.Cancel();

            return notificationResult;            
        }

    }

    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var notificationId = intent.Extras.GetInt(NotificationBuilder.NotificationId).ToString();

            switch (intent.Action)
            {
                case NotificationBuilder.OnClickIntent:
                    NotificationBuilder.EventResult.Add(notificationId, NotificationResult.Clicked);                   
                    break;
                default:
                case NotificationBuilder.DismissedClickIntent:
                    NotificationBuilder.EventResult.Add(notificationId, NotificationResult.Dismissed);
                    break;
            }

            NotificationBuilder.ResetEvent[notificationId].Set();
        }
    }

}