using Android.Widget;

namespace Plugin.Toasts
{
    using Android.App;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class ToastNotification : IToastNotificator
    {
        static Activity _activity = null;
        static IPlatformOptions _androidOptions;
        static SnackbarNotification _snackbarNotification;
        static NotificationBuilder _notificationBuilder;

        public static void Init(Activity activity, IPlatformOptions androidOptions = null)
        {
            _activity = activity;
            _snackbarNotification = new SnackbarNotification();
            _notificationBuilder = new NotificationBuilder();

            if (androidOptions == null)
                _androidOptions = new PlatformOptions() { Style = NotificationStyle.Default, SmallIconDrawable = Android.Resource.Drawable.IcDialogInfo };
            else
                _androidOptions = androidOptions;

            _notificationBuilder.Init(_androidOptions);
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
            Task.Run(async () =>
            {
                return await Notify(options);
            }).ContinueWith((task) =>
            {
                var tResult = task.Result;
                if (options.AndroidOptions.DebugShowCallbackToast)
                {
                    _activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(Application.Context, "Notification [" + tResult.Id + "] Result Action: " + tResult.Action, ToastLength.Short).Show();
                    });
                }
                callback.Invoke(tResult);
            });
        }

        public void CancelAllDelivered()
        {
            _notificationBuilder.CancelAll();
            _snackbarNotification.CancelAll();
        }

        /// <summary>
        /// Available on >= API23 (Android 6.0) as is.
        /// Not Available on >= API23, will return empty list
        /// </summary>
        /// <returns></returns>
        public Task<IList<INotification>> GetDeliveredNotifications()
        {
            return Task.FromResult(_notificationBuilder.GetDeliveredNotifications());
        }

    }

}