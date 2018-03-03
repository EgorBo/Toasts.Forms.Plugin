namespace Plugin.Toasts
{
    using Android.App;
    using Android.Support.Design.Widget;
    using Android.Text;
    using Android.Views;
    using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
    using System.Threading;

    internal class SnackbarNotification
    {
        private IDictionary<string, ManualResetEvent> _resetEvents = new ConcurrentDictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new ConcurrentDictionary<string, NotificationResult>();
        private IList<Snackbar> _snackBars = new List<Snackbar>();

        private int _count = 0;
        private object _lock = new object();

        public NotificationResult Notify(Activity activity, INotificationOptions options)
        {
            var view = activity.FindViewById(Android.Resource.Id.Content);

            SpannableStringBuilder builder = new SpannableStringBuilder();

            builder.Append(options.Title);

            if (!string.IsNullOrEmpty(options.Title) && !string.IsNullOrEmpty(options.Description))
                builder.Append("\n"); // Max of 2 lines for snackbar

            builder.Append(options.Description);

            var id = _count.ToString();
            _count++;

            var snackbar = Snackbar.Make(view, builder, Snackbar.LengthLong);
            if (options.IsClickable)
                snackbar.SetAction(options.AndroidOptions.ViewText, new EmptyOnClickListener(id, (toastId, result) => { ToastClosed(toastId, result); }, new NotificationResult() { Action = NotificationAction.Clicked }));
            else
                snackbar.SetAction(options.AndroidOptions.DismissText, new EmptyOnClickListener(id, (toastId, result) => { ToastClosed(toastId, result); }, new NotificationResult() { Action = NotificationAction.Dismissed }));

            // Monitor callbacks
            snackbar.SetCallback(new ToastCallback(id, (toastId, result) => { ToastClosed(toastId, result); }));

            // Setup reset events
            var resetEvent = new ManualResetEvent(false);
            _resetEvents.Add(id, resetEvent);
            _snackBars.Add(snackbar);
            snackbar.Show();

            resetEvent.WaitOne(); // Wait for a result

            var notificationResult = _eventResult[id];

            _eventResult.Remove(id);
            _resetEvents.Remove(id);

            if (_snackBars.Contains(snackbar))
                _snackBars.Remove(snackbar);

            return notificationResult;
        }

        public void CancelAll()
        {
            foreach (var snackbar in _snackBars)
                snackbar.Dismiss();
        }

        private void ToastClosed(string id, NotificationResult result)
        {
			lock (_lock)
			{
				if (_resetEvents.ContainsKey(id))
				{
					_eventResult.Add(id, result);
					_resetEvents[id].Set();
				}
			}
        }
    }

    internal class ToastCallback : Snackbar.Callback
    {
        private string _id = "";
        private Action<string, NotificationResult> _callback;
        public ToastCallback(string id, Action<string, NotificationResult> callback)
        {
            _id = id;
            _callback = callback;
        }

        public override void OnDismissed(Snackbar snackbar, int evt)
        {
            switch (evt)
            {
                case DismissEventAction:
                    return;  // Handled via OnClickListeners
                case DismissEventConsecutive:
                case DismissEventManual:
                case DismissEventSwipe:
                    _callback(_id, new NotificationResult() { Action = NotificationAction.Dismissed });
                    break;
                case DismissEventTimeout:
                default:
                    _callback(_id, new NotificationResult() { Action = NotificationAction.Timeout });
                    break;
            }
        }
    }

    internal class EmptyOnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private string _id = "";
        private Action<string, NotificationResult> _callback;
        private NotificationResult _result = new NotificationResult() { Action = NotificationAction.Dismissed };
        public EmptyOnClickListener(string id, Action<string, NotificationResult> callback, NotificationResult result)
        {
            _id = id;
            _callback = callback;
            _result = result;
        }
        public void OnClick(View v)
        {
            _callback(_id, _result);
        }
    }
}