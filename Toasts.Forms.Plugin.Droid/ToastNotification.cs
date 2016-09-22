namespace Plugin.Toasts
{
    using Android.App;
    using Android.Provider;
    using Android.Support.Design.Widget;
    using Android.Text;
    using Android.Text.Style;
    using System.Threading.Tasks;
    using Android.Views;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class ToastNotification : IToastNotificator
    {
        private static Activity _activity = null;
        private int _count = 0;
        private object _lock = new object();
        private IDictionary<string, ManualResetEvent> _resetEvents = new Dictionary<string, ManualResetEvent>();
        private IDictionary<string, NotificationResult> _eventResult = new Dictionary<string, NotificationResult>();

        public static void Init(Activity activity)
        {
            _activity = activity;
        }

        public async Task<NotificationResult> Notify(INotificationOptions options)
        {
            return await Task.Run(() =>
            {
                var view = _activity.FindViewById(Android.Resource.Id.Content);

                SpannableStringBuilder builder = new SpannableStringBuilder();
                int spannableWidth = 0;
                if (!string.IsNullOrEmpty(options.LogoUri))
                {
                    builder.Append(" "); // Because the ImageSpan is spanning over this text.

                    var imageName = options.LogoUri;
                    if (!options.LogoUri.Contains("//") && options.LogoUri.Contains("."))
                        imageName = options.LogoUri.Substring(0, options.LogoUri.IndexOf("."));

                    var bitmap = MediaStore.Images.Media.GetBitmap(_activity.ContentResolver, Android.Net.Uri.Parse($"android.resource://{_activity.PackageName}/drawable/{imageName}"));

                    ImageSpan imageSpan = new ImageSpan(bitmap, SpanAlign.Bottom);
                    imageSpan.Drawable.SetBounds(0, 0, bitmap.Width, bitmap.Height); //TODO: check in different DPI's if this is constant
                    builder.SetSpan(imageSpan, 0, 1, SpanTypes.ExclusiveExclusive);
                    spannableWidth = bitmap.Width;
                }

                builder.Append(options.Title);

                if (!string.IsNullOrEmpty(options.Title) && !string.IsNullOrEmpty(options.Description))
                    builder.Append("\n"); // Max of 2 lines for snackbar

                if (spannableWidth > 0)
                {
                    builder.Append(" ");
                    // There is an image and we need to align the next line with the Title
                    ImageSpan blankImage = new ImageSpan((Android.Graphics.Bitmap)null);
                    blankImage.Drawable.SetBounds(0, 0, spannableWidth, 0);
                    builder.SetSpan(blankImage, builder.Length() - 1, builder.Length(), SpanTypes.ExclusiveExclusive);
                }

                builder.Append(options.Description);

                var id = _count.ToString();
                _count++;

                var snackbar = Snackbar.Make(view, builder, Snackbar.LengthLong);
                if (options.IsClickable)
                    snackbar.SetAction("View", new EmptyOnClickListener(id, (toastId, result) => { ToastClosed(toastId, result); }, NotificationResult.Clicked));
                else
                    snackbar.SetAction("Dismiss", new EmptyOnClickListener(id, (toastId, result) => { ToastClosed(toastId, result); }, NotificationResult.Dismissed));

                // Monitor callbacks
                snackbar.SetCallback(new ToastCallback(id, (toastId, result) => { ToastClosed(toastId, result); }));

                // Setup reset events
                var resetEvent = new ManualResetEvent(false);
                _resetEvents.Add(id, resetEvent);

                snackbar.Show();

                resetEvent.WaitOne(); // Wait for a result

                var notificationResult = _eventResult[id];

                _eventResult.Remove(id);
                _resetEvents.Remove(id);

                return notificationResult;
            });

        }
        private void ToastClosed(string id, NotificationResult result)
        {
            lock (_lock)
            {
                _eventResult.Add(id, result);
                _resetEvents[id].Set();
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
                    _callback(_id, NotificationResult.Dismissed);
                    break;
                case DismissEventTimeout:
                default:
                    _callback(_id, NotificationResult.Timeout);
                    break;
            }
        }
    }


    internal class EmptyOnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private string _id = "";
        private Action<string, NotificationResult> _callback;
        private NotificationResult _result = NotificationResult.Dismissed;
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