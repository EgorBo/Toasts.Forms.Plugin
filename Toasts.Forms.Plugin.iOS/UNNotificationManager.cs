namespace Plugin.Toasts
{
    using Foundation;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UserNotifications;

    public class UNNotificationManager
    {
        private IDictionary<string, ManualResetEvent> _resetEvents = new Dictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new Dictionary<string, NotificationResult>();
        private int _count = 0;
        private static object _lock = new object();

        public INotificationResult Notify(INotificationOptions options)
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent();
            content.Title = options.Title;
            content.Body = options.Description;
            content.Sound = UNNotificationSound.Default;
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false); // Trigger now

            var id = _count.ToString();
            _count++;

            var request = UNNotificationRequest.FromIdentifier(id, content, trigger);
            notificationCenter.Delegate = new UserNotificationCenterDelegate(id, (identifier, notificationResult) =>
            {
                lock (_lock)
                    if (_resetEvents.ContainsKey(identifier) && !_eventResult.ContainsKey(identifier))
                    {
                        _eventResult.Add(identifier, notificationResult);
                        _resetEvents[identifier].Set();
                    }
            });

            var resetEvent = new ManualResetEvent(false);
            _resetEvents.Add(id, resetEvent);

            notificationCenter.AddNotificationRequest(request, (error) =>
            {
                if (error != null)
                    _eventResult.Add(request.Identifier, new NotificationResult() { Action = NotificationAction.Failed });
            });

            resetEvent.WaitOne();

            var result = _eventResult[id];

            _resetEvents.Remove(id);
            _eventResult.Remove(id);

            return result;
        }

        internal class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
        {
            private Action<string, NotificationResult> _action;
            private string _id;
            public UserNotificationCenterDelegate(string id, Action<string, NotificationResult> action)
            {
                _action = action;
                _id = id;
            }
          
            public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            {
                // Timer here for a timeout since no Toast Dismissed Event (7 seconds til auto dismiss)
                var timer = NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(7), (nsTimer) =>
                {
                    _action(_id, new NotificationResult() { Action = NotificationAction.Timeout });
                    nsTimer.Invalidate();
                });

                // Shows toast on screen
                completionHandler(UNNotificationPresentationOptions.Alert);
            }


            public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
            {
                // I Clicked it :)
                _action(_id, new NotificationResult() { Action = NotificationAction.Clicked });
            }

        }
    }
}
