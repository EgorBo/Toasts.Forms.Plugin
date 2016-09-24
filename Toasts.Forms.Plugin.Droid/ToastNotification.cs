namespace Plugin.Toasts
{
    using Android.App;
    using System;
    using System.Threading.Tasks;

    public class ToastNotification : IToastNotificator
    {
        private static Activity _activity = null;
        private static IPlatformOptions _androidOptions;
        private static SnackbarNotification _snackbarNotification;
        private static NotificationBuilder _notificationBuilder;

        public static void Init(Activity activity, IPlatformOptions androidOptions = null)
        {
            _activity = activity;
            _snackbarNotification = new SnackbarNotification();
            _notificationBuilder = new NotificationBuilder();

            if (androidOptions == null)
                _androidOptions = new PlatformOptions() { Style = NotificationStyle.Default, SmallIconDrawable = Android.Resource.Drawable.IcDialogInfo };
            else
                _androidOptions = androidOptions;

            _notificationBuilder.Init(_activity, _androidOptions);
        }

        public async Task<INotificationResult> Notify(INotificationOptions options)
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
        public void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () => callback(await Notify(options)));
        }

    }

}