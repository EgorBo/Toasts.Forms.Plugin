namespace Plugin.Toasts
{
    using Extensions;
    using Foundation;
    using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
    using System.Threading;
    using UserNotifications;

    public class UNNotificationManager
    {
        private IDictionary<string, ManualResetEvent> _resetEvents = new ConcurrentDictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new ConcurrentDictionary<string, NotificationResult>();
        private int _count = 0;
        private static object _lock = new object();

        public INotificationResult Notify(INotificationOptions options)
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent();
            content.Title = options.Title;
            content.Body = options.Description;
            content.Sound = UNNotificationSound.Default;

            if (options.iOSOptions != null && options.iOSOptions.SetBadgeCount)
               content.Badge = options.iOSOptions.BadgeCount;

            UNNotificationTrigger trigger;

            if (options.DelayUntil.HasValue)
                trigger = UNCalendarNotificationTrigger.CreateTrigger(options.DelayUntil.Value.ToNSDateComponents(), false);
            else
                trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false);

            var id = _count.ToString();
            _count++;

            var request = UNNotificationRequest.FromIdentifier(id, content, trigger);
            notificationCenter.Delegate = new UserNotificationCenterDelegate(id, (identifier, notificationResult) =>
            {
                lock (_lock)
                    if (_resetEvents?.ContainsKey(identifier) == true && _eventResult?.ContainsKey(identifier) == false)
                    {
                        _eventResult.Add(identifier, notificationResult);
                        _resetEvents[identifier].Set();
                    }
            }, options.ClearFromHistory, options.AllowTapInNotificationCenter);

            var resetEvent = new ManualResetEvent(false);
            _resetEvents.Add(id, resetEvent);

            notificationCenter.AddNotificationRequest(request, (error) =>
            {
                if (error != null)
                    _eventResult?.Add(request.Identifier, new NotificationResult() { Action = NotificationAction.Failed });
            });

            if (options.DelayUntil.HasValue)
                return new NotificationResult() { Action = NotificationAction.NotApplicable };

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
            private bool _cancel;
            private bool _allowTapInNotificationCenter;
            public UserNotificationCenterDelegate(string id, Action<string, NotificationResult> action, bool cancel, bool allowTapInNotificationCenter)
            {
                _action = action;
                _id = id;
                _cancel = cancel;
                _allowTapInNotificationCenter = allowTapInNotificationCenter;
            }

            public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            {
                // Timer here for a timeout since no Toast Dismissed Event (7 seconds til auto dismiss)
                var timer = NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(7), (nsTimer) =>
                {
                    if (_cancel || !_allowTapInNotificationCenter)
                        _action(_id, new NotificationResult() { Action = NotificationAction.Timeout });

                    if (_cancel) // Clear notification from list
                        UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new string[] { _id });

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
