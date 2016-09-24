namespace Plugin.Toasts
{
    using Android.App;
    using System.Threading.Tasks;

    public class ToastNotification : IToastNotificator
    {
        private static Activity _activity = null;
        private static IAndroidOptions _androidOptions;
        private static SnackbarNotification _snackbarNotification;
        private static NotificationBuilder _notificationBuilder;

        public static void Init(Activity activity, IAndroidOptions androidOptions = null)
        {
            _activity = activity;
            _snackbarNotification  = new SnackbarNotification();
            _notificationBuilder = new NotificationBuilder();
            _notificationBuilder.Init(_activity);

            if (androidOptions == null)
                _androidOptions = new AndroidOptions() { Style = NotificationStyle.Default };
            else
                _androidOptions = androidOptions;
        }

        public async Task<NotificationResult> Notify(INotificationOptions options)
        {           
            return await Task.Run(() =>
            {
              switch (_androidOptions.Style)
                {
                    case NotificationStyle.Notifications:
                        return _notificationBuilder.Notify(_activity, options);
                    case NotificationStyle.Snackbar:
                        return _snackbarNotification.Notify(_activity, options);
                    default:
                    case NotificationStyle.Default:
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                            return _notificationBuilder.Notify(_activity, options);
                        else
                            return _snackbarNotification.Notify(_activity, options);                        
                }
            });
        }
      
    }
    
}