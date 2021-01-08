namespace Plugin.Toasts
{
	using Extensions;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using UserNotifications;

	public class ToastNotification : IToastNotificator
    {
        private UNNotificationManager _notificationManager;

        public ToastNotification()
        {
            _notificationManager = new UNNotificationManager();
        }

        public static void Init() { }

        public async Task<INotificationResult> Notify(INotificationOptions options)
        {
            return await Task.Run(() =>
            {
                return _notificationManager.Notify (options);
            });
        }

        public void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () => callback(await Notify(options)));
        }

        public async Task<IList<INotification>> GetDeliveredNotifications()
        {
            IList<INotification> list = new List<INotification>();

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

            return list;
        }

        public void CancelAllDelivered()
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            notificationCenter.RemoveAllDeliveredNotifications();
        }

        public void SystemEvent(object args)
        {
            throw new NotSupportedException();
        }
    }


}
