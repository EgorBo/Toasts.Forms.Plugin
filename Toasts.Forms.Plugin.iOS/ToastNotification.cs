namespace Plugin.Toasts
{
	using Extensions;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using UIKit;
	using UserNotifications;

	public class ToastNotification : IToastNotificator
    {
        private UNNotificationManager _notificationManager;
        private LocalNotificationManager _localNotificationManager;

        public ToastNotification()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                _notificationManager = new UNNotificationManager();
            else
                _localNotificationManager = new LocalNotificationManager();
        }

        public static void Init() { }

        public async Task<INotificationResult> Notify(INotificationOptions options)
        {
            INotificationResult result = null;
            return await Task.Run(() =>
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                    return _notificationManager.Notify(options);
                else
                {
                    ManualResetEvent reset = new ManualResetEvent(false);
                    UIApplication.SharedApplication.InvokeOnMainThread(() => { result = _localNotificationManager.Notify(options); reset.Set(); });
                    reset.WaitOne();
                    return result;
                }

            });
        }

        public void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () => callback(await Notify(options)));
        }

        public async Task<IList<INotification>> GetDeliveredNotifications()
        {
            IList<INotification> list = new List<INotification>();

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var notificationCenter = UNUserNotificationCenter.Current;

                var deliveredNotifications = await notificationCenter.GetDeliveredNotificationsAsync();

                foreach (var notification in deliveredNotifications)
                {

                    UNNotificationContent content = notification.Request.Content;
                    list.Add(new Notification()
                    {
                        Id = notification.Request.Identifier,
                        Title = content.Title,
                        Description = content.Body,
                        Delivered = notification.Date.ToDateTime()
                    });
                }
            }

            return list;
        }

        public void CancelAllDelivered()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var notificationCenter = UNUserNotificationCenter.Current;

                notificationCenter.RemoveAllDeliveredNotifications();
            }
        }

        public void SystemEvent(object args)
        {
            throw new NotSupportedException();
        }
    }


}
