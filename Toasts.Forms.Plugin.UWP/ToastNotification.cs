#if WINDOWS_UWP
namespace Toasts.Forms.Plugin.UWP
#else
namespace Toasts.Forms.Plugin.WinRT
#endif
{
    using global::Plugin.Toasts;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.UI.Notifications;

    public class ToastNotification : IToastNotificator
    {

        private IDictionary<string, ManualResetEvent> _resetEvents = new Dictionary<string, ManualResetEvent>();
        private IDictionary<string, bool> _eventResult = new Dictionary<string, bool>();
        private int _count = 0;

#if WINRT
        private IDictionary<string, Windows.UI.Notifications.ToastNotification> _toasts = new Dictionary<string, Windows.UI.Notifications.ToastNotification>();
#endif

        public Task<bool> Notify(INotificationOptions options)
        {
            return Task.Run(() =>
            {
                ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
                Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
                toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(options.Title));
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(options.Description));
                Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");

                if (options.LogoSource != null)
                {
                    Windows.Data.Xml.Dom.XmlElement image = toastXml.CreateElement("image");
                    image.SetAttribute("placement", "appLogoOverride");
                    image.SetAttribute("src", options.LogoSource.ToString());

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

                var waitEvent = new ManualResetEvent(false);

                _resetEvents.Add(id, waitEvent);

                ToastNotifier.Show(toast);

                waitEvent.WaitOne();

                var result = _eventResult[id];

                _resetEvents.Remove(id);
                _eventResult.Remove(id);

#if WINRT
                _toasts.Remove(id);
#endif
                return result;

            });
        }

        private void Toast_Activated(Windows.UI.Notifications.ToastNotification sender, object args)
        {
#if WINDOWS_UWP
           var id = sender.Tag;
#else
            var id = _toasts.Single(x => x.Value == sender).Key;
#endif
            _eventResult.Add(id, true);
            _resetEvents[id].Set();
        }

        private void Toast_Dismissed(Windows.UI.Notifications.ToastNotification sender, ToastDismissedEventArgs args)
        {
#if WINDOWS_UWP
           var id = sender.Tag;
#else
            var id = _toasts.Single(x => x.Value == sender).Key;
#endif
            _eventResult.Add(id, false);
            _resetEvents[id].Set();
        }

    }
}
