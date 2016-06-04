using System;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using Java.Interop;
using Java.Util;
using Java.Util.Concurrent;

namespace Plugin.Toasts
{
    public class Manager : Handler
    {
        private const int DISPLAY_CROUTON = 0xc2007;
        private const int ADD_CROUTON_TO_VIEW = 0xc20071;
        private const int REMOVE_CROUTON = 0xc20072;

        private static Manager _instance;
        private readonly IQueue _croutonQueue;

        private Manager()
        {
            _croutonQueue = new LinkedBlockingQueue();
        }

        public static Manager Instance
        {
            get { return _instance ?? (_instance = new Manager()); } 
        }

        public void Add(Crouton crouton)
        {
            _croutonQueue.Add(crouton);
            DisplayCrouton();
        }

        private void DisplayCrouton()
        {
            if (_croutonQueue.IsEmpty)
            {
                return;
            }

            // First peek whether the Crouton has an activity.
            var currentCrouton = _croutonQueue.Peek().JavaCast<Crouton>();

            // If the activity is null we poll the Crouton off the queue.
            if (null == currentCrouton.GetActivity())
            {
                _croutonQueue.Poll();
            }

            if (!currentCrouton.IsShowing())
            {
                // Display the Crouton
                SendMessage(currentCrouton, Convert.ToInt32(ADD_CROUTON_TO_VIEW));
                currentCrouton.OnDisplayed();
            }
            else
            {
                SendMessageDelayed(currentCrouton, DISPLAY_CROUTON, CalculateCroutonDuration(currentCrouton));
            }
        }

        private long CalculateCroutonDuration(Crouton crouton)
        {
            long croutonDuration = crouton.DurationInMilliseconds;
            croutonDuration += crouton.GetInAnimation().Duration;
            croutonDuration += crouton.GetOutAnimation().Duration;
            return croutonDuration;
        }

        private void SendMessage(Crouton crouton, int messageId)
        {
            Message message = ObtainMessage(messageId);
            message.Obj = crouton;
            SendMessage(message);
        }

        private void SendMessageDelayed(Crouton crouton, Int64 messageId, long delay)
        {
            Message message = ObtainMessage((int) messageId);
            message.Obj = crouton;
            SendMessageDelayed(message, delay);
        }
        
        public override void HandleMessage(Message message)
        {
            var crouton = (Crouton) message.Obj;
            if (null == crouton)
            {
                return;
            }
            switch (message.What)
            {
                case DISPLAY_CROUTON:
                    DisplayCrouton();
                    break;

                case ADD_CROUTON_TO_VIEW:
                    AddCroutonToView(crouton);
                    break;

                case REMOVE_CROUTON:
                    RemoveCrouton(crouton);
                    crouton.OnRemoved();
                    break;

                default:
                    base.HandleMessage(message);
                    break;
            }
        }

        private void AddCroutonToView(Crouton crouton)
        {
            // don't Add if it is already showing
            if (crouton.IsShowing())
            {
                return;
            }

            View croutonView = crouton.GetView();
            if (null == croutonView.Parent)
            {
                ViewGroup.LayoutParams parameters = croutonView.LayoutParameters;
                if (null == parameters)
                {
                    parameters = new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.WrapContent);
                }

                Activity activity = crouton.GetActivity();
                if (null == activity || activity.IsFinishing)
                {
                    return;
                }
                HandleTranslucentActionBar((ViewGroup.MarginLayoutParams) parameters, activity);
                HandleActionBarOverlay((ViewGroup.MarginLayoutParams) parameters, activity);

                activity.AddContentView(croutonView, parameters);
            }

            croutonView.RequestLayout(); // This is needed so the animation can use the measured with/height
            ViewTreeObserver observer = croutonView.ViewTreeObserver;
            if (null != observer)
            {
                CallOnGlobalLayout(crouton, croutonView);
            }
        }

        [TargetApi(Value = 16)]
        private void CallOnGlobalLayout(Crouton crouton, View croutonView)
        {
            var layoutListener = new GlobalLayoutListener();
            layoutListener.OnGlobalLayout(delegate
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
                {
                    croutonView.ViewTreeObserver.RemoveGlobalOnLayoutListener(layoutListener);
                }
                else
                {
                    croutonView.ViewTreeObserver.RemoveOnGlobalLayoutListener(layoutListener);
                }

                if (crouton.GetInAnimation() != null)
                {
                    croutonView.StartAnimation(crouton.GetInAnimation());
                    AnnounceForAccessibilityCompat(crouton.GetActivity(), crouton.DataContext == null ? "NULL" : crouton.DataContext.ToString());
                    SendMessageDelayed(crouton, REMOVE_CROUTON,
                    crouton.DurationInMilliseconds + crouton.GetInAnimation().Duration);
                }
            });
        }

        [TargetApi(Value = 19)]
        private void HandleTranslucentActionBar(ViewGroup.MarginLayoutParams parameters, Activity activity)
        {
            // Translucent status is only available as of Android 4.4 Kit Kat.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                var flags = (int) activity.Window.Attributes.Flags;
                var translucentStatusFlag = (int) WindowManagerFlags.TranslucentStatus;
                if ((flags & translucentStatusFlag) == translucentStatusFlag)
                {
                    SetActionBarMargin(parameters, activity);
                }
            }
        }

        [TargetApi(Value = 11)]
        private void HandleActionBarOverlay(ViewGroup.MarginLayoutParams parameters, Activity activity)
        {
            // ActionBar overlay is only available as of Android 3.0 Honeycomb.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
            {
                bool flags = activity.Window.HasFeature(WindowFeatures.ActionBarOverlay);
                if (flags)
                {
                    SetActionBarMargin(parameters, activity);
                }
            }
        }

        private void SetActionBarMargin(ViewGroup.MarginLayoutParams parameters, Activity activity)
        {
            int actionBarContainerId = Android.Content.Res.Resources.System.GetIdentifier("action_bar_container", "id", "android");
            View actionBarContainer = activity.FindViewById(actionBarContainerId);
            // The action bar is present: the app is using a Holo theme.
            if (null != actionBarContainer)
            {
                parameters.TopMargin = actionBarContainer.Bottom;
            }
        }

        public void RemoveCrouton(Crouton crouton)
        {
            View croutonView = crouton.GetView();
            var croutonParentView = (ViewGroup) croutonView.Parent;

            if (null != croutonParentView)
            {
                croutonView.StartAnimation(crouton.GetOutAnimation());

                // Remove the Crouton from the queue.
                var removed = (Crouton) _croutonQueue.Poll();

                // Remove the crouton from the view's parent.
                croutonParentView.RemoveView(croutonView);
                if (null != removed)
                {
                    removed.DetachActivity();
                    removed.OnRemoved();
                    removed.DetachLifecycleCallback();
                }

                // Send a message to display the next crouton but delay it by the out
                // animation duration to Make sure it finishes
                SendMessageDelayed(crouton, DISPLAY_CROUTON, crouton.GetOutAnimation().Duration);
            }
        }
       
        public static void AnnounceForAccessibilityCompat(Context context, String text)
        {
            if ((int) Build.VERSION.SdkInt >= 4)
            {
                AccessibilityManager accessibilityManager = null;
                if (null != context)
                {
                    accessibilityManager = (AccessibilityManager) context.GetSystemService(Context.AccessibilityService);
                }
                if (null == accessibilityManager || !accessibilityManager.IsEnabled)
                {
                    return;
                }

                // Prior to SDK 16, announcements could only be made through FOCUSED
                // events. Jelly Bean (SDK 16) added support for speaking text verbatim
                // using the ANNOUNCEMENT event type.
                EventTypes eventType;
                if ((int) Build.VERSION.SdkInt < 16)
                {
                    eventType = EventTypes.ViewFocused;
                }
                else
                {
                    eventType = EventTypes.Announcement;
                }

                // Construct an accessibility event with the minimum recommended
                // attributes. An event without a class name or package may be dropped.
                AccessibilityEvent ev = AccessibilityEvent.Obtain(eventType);
                var textProxy = new Java.Lang.String(text);
                ev.Text.Add(textProxy);
                ev.ClassName = _instance.GetType().ToString();
                ev.PackageName = context.PackageName;

                // Sends the event directly through the accessibility manager. If your
                // application only targets SDK 14+, you should just call
                // getParent().requestSendAccessibilityEvent(this, event);
                accessibilityManager.SendAccessibilityEvent(ev);
            }
        }

        private class GlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            public void OnGlobalLayout(System.Action action)
            {
                action();
            }

            public void OnGlobalLayout() { }
        }

        public void RemoveCroutons()
        {
			while (!_croutonQueue.IsEmpty)
			{
				RemoveCrouton((Crouton) _croutonQueue.Peek());
			}
        }
    }
}