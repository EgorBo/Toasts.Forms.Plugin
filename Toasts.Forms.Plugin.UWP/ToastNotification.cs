#if WINDOWS_UWP
namespace Plugin.Toasts.UWP
{
#else
namespace Plugin.Toasts.WinRT
{
    using System.Linq;
#endif

    using Toasts;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.UI.Notifications;
    using System;

    public class ToastNotification : IToastNotificator
    {

        private IDictionary<string, ManualResetEvent> _resetEvents = new Dictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new Dictionary<string, NotificationResult>();
        private int _count = 0;

#if WINRT
        private IDictionary<string, Windows.UI.Notifications.ToastNotification> _toasts = new Dictionary<string, Windows.UI.Notifications.ToastNotification>();
#endif

        public static void Init() { }

        public Task<INotificationResult> Notify(INotificationOptions options)
        {
            return Task.Run(() =>
            {
                ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
                Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
                toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(options.Title));
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(options.Description));
                Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");

                if (!string.IsNullOrEmpty(options.WindowsOptions.LogoUri))
                {
                    Windows.Data.Xml.Dom.XmlElement image = toastXml.CreateElement("image");
                    image.SetAttribute("placement", "appLogoOverride");

                    var imageUri = options.WindowsOptions.LogoUri;
                    if (!options.WindowsOptions.LogoUri.Contains("//"))
                        imageUri = $"ms-appx:///{options.WindowsOptions.LogoUri}";

                    image.SetAttribute("src", imageUri);

                    toastXml.GetElementsByTagName("binding")[0].AppendChild(image);
                    toastXml.GetElementsByTagName("binding")[0].Attributes[0].InnerText = "ToastGeneric";
                }

                Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(toastXml);
                var id = _count.ToString();
#if WINDOWS_UWP
                toast.Tag = id;
#else
                _toasts.Add(id, toast);
#endif
                _count++;
                toast.Dismissed += Toast_Dismissed;
                toast.Activated += Toast_Activated;
                toast.Failed += Toast_Failed;

                var waitEvent = new ManualResetEvent(false);

                _resetEvents.Add(id, waitEvent);

                ToastNotifier.Show(toast);

                waitEvent.WaitOne();

                INotificationResult result = _eventResult[id];

                if (!options.IsClickable && result.Action == NotificationAction.Clicked) // A click is transformed to manual dismiss
                    result = new NotificationResult() { Action = NotificationAction.Dismissed };

                _resetEvents.Remove(id);
                _eventResult.Remove(id);

#if WINRT
                _toasts.Remove(id);
#endif
                return result;

            });
        }

        private void Toast_Failed(Windows.UI.Notifications.ToastNotification sender, ToastFailedEventArgs args)
        {
#if WINDOWS_UWP
            var id = sender.Tag;
#else
            var id = _toasts.Single(x => x.Value == sender).Key;
#endif
            _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Failed });
            _resetEvents[id].Set();
        }

        private void Toast_Activated(Windows.UI.Notifications.ToastNotification sender, object args)
        {
#if WINDOWS_UWP
            var id = sender.Tag;
#else
            var id = _toasts.Single(x => x.Value == sender).Key;
#endif
            _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Clicked });
            _resetEvents[id].Set();
        }

        private void Toast_Dismissed(Windows.UI.Notifications.ToastNotification sender, ToastDismissedEventArgs args)
        {
#if WINDOWS_UWP
            var id = sender.Tag;
#else
            var id = _toasts.Single(x => x.Value == sender).Key;
#endif
            switch (args.Reason)
            {
                case ToastDismissalReason.ApplicationHidden:
                    _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.ApplicationHidden });
                    break;
                case ToastDismissalReason.TimedOut:
                    _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Timeout });
                    break;
                case ToastDismissalReason.UserCanceled:
                default:
                    _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Dismissed });
                    break;
            }

            _resetEvents[id].Set();
        }

        public void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () => callback(await Notify(options)));
        }

        /// <summary>
        /// Delivered Notifications for UWP that have not been dismissed.
        /// Not Available for WinRT.
        /// </summary>
        /// <returns></returns>
        public Task<IList<INotification>> GetDeliveredNotifications()
        {

#if WINDOWS_UWP
            IList<INotification> notifications = new List<INotification>();

            foreach (var notification in ToastNotificationManager.History.GetHistory())
                notifications.Add(new Toasts.Notification()
                {
                    Id = notification.Tag,
                    Title = notification.Content.GetElementsByTagName("text")[0].InnerText,
                    Description = notification.Content.GetElementsByTagName("text")[1].InnerText,
                    Delivered = DateTime.MinValue // Unknown
                });

            return Task.FromResult(notifications);
#else
            IList<INotification> notifications = new List<INotification>();
            return Task.FromResult(notifications);
#endif

        }
    }
}
