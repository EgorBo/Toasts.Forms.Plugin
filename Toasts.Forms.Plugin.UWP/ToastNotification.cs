namespace Plugin.Toasts.UWP
{
    using Toasts;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.UI.Notifications;
    using System;
    using Windows.ApplicationModel.Activation;
    using System.Xml;
	using System.Collections.Concurrent;

	public class ToastNotification : IToastNotificator
    {

        private IDictionary<string, ManualResetEvent> _resetEvents = new ConcurrentDictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new ConcurrentDictionary<string, NotificationResult>();
        private IDictionary<string, INotificationOptions> _notificationOptions = new ConcurrentDictionary<string, INotificationOptions>();
        private int _count = 0;

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

                var id = _count.ToString();

                var root = toastXml.DocumentElement;
                root.SetAttribute("launch", id.ToString());

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

                if (options.DelayUntil.HasValue)
                {
                    ScheduledToastNotification toast = new ScheduledToastNotification(toastXml, options.DelayUntil.Value);
                    ToastNotifier.AddToSchedule(toast);
                    return new NotificationResult() { Action = NotificationAction.NotApplicable };
                }
                else
                {
                    Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(toastXml);


                    toast.Tag = id;
                    _count++;
                    toast.Dismissed += Toast_Dismissed;
                    toast.Activated += Toast_Activated;
                    toast.Failed += Toast_Failed;
                    _notificationOptions.Add(id, options);

                    var waitEvent = new ManualResetEvent(false);

                    _resetEvents.Add(id, waitEvent);

                    ToastNotifier.Show(toast);

                    waitEvent.WaitOne();

                    INotificationResult result = _eventResult[id];

                    if (!options.IsClickable && result.Action == NotificationAction.Clicked) // A click is transformed to manual dismiss
                        result = new NotificationResult() { Action = NotificationAction.Dismissed };

                    if (_resetEvents.ContainsKey(id))
                        _resetEvents.Remove(id);
                    if (_eventResult.ContainsKey(id))
                        _eventResult.Remove(id);
                    if (_notificationOptions.ContainsKey(id))
                        _notificationOptions.Remove(id);

                    return result;
                }

            });
        }

        private void Toast_Failed(Windows.UI.Notifications.ToastNotification sender, ToastFailedEventArgs args)
        {
            var id = sender.Tag;
            _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Failed });
            _resetEvents[id].Set();
        }
        private object _eventLock = new object();

        private void Toast_Activated(Windows.UI.Notifications.ToastNotification sender, object args)
        {
            lock (_eventLock)
            {
                var id = sender.Tag;

                if (!_eventResult.ContainsKey(id))
                    _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Clicked });

                if (_resetEvents.ContainsKey(id))
                    _resetEvents[id].Set();
            }
        }

        private void Toast_Dismissed(Windows.UI.Notifications.ToastNotification sender, ToastDismissedEventArgs args)
        {
            var id = sender.Tag;
            var options = _notificationOptions[id];

            if (args.Reason != ToastDismissalReason.UserCanceled && options.AllowTapInNotificationCenter)
                return;

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
            if (_notificationOptions.ContainsKey(id))
            {
                if (options.ClearFromHistory)
                    ToastNotificationManager.History.Remove(id);

                _notificationOptions.Remove(id);
            }

            _resetEvents[id].Set();
        }

        public void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () =>
            callback(await Notify(options))
            );
        }

        /// <summary>
        /// Delivered Notifications for UWP that have not been dismissed.
        /// </summary>
        /// <returns></returns>
        public Task<IList<INotification>> GetDeliveredNotifications()
        {
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
        }

        public void CancelAllDelivered()
        {
            ToastNotificationManager.History.Clear();
        }

        public void SystemEvent(object args)
        {
            if (args is ToastNotificationActivatedEventArgs toastArgs)
            {
                lock (_eventLock)
                {
                    // Toast notification was clicked, 
                    var id = toastArgs.Argument;
                    if (_notificationOptions.ContainsKey(id))
                    {
                        var options = _notificationOptions[id];

                        if (options.ClearFromHistory)
                            ToastNotificationManager.History.Remove(id);

                        _notificationOptions.Remove(id);
                    }

                    if (!_eventResult.ContainsKey(id))
                        _eventResult.Add(id, new NotificationResult() { Action = NotificationAction.Clicked });

                    if (_resetEvents.ContainsKey(id))
                        _resetEvents[id].Set();
                }
            }
        }
    }
}
