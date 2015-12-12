using System;
using System.Threading.Tasks;
using Android.App;
using Toasts;
using View = Android.Views.View;
using System.Threading;
using Android.Content;

namespace Toasts
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        private static Context _context = null;
        public ToastNotificatorImplementation()
        {
            
        }

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null, bool clickable = true)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Activity currentActivity = _context as Activity;
            if (currentActivity == null)
                return Task.FromResult(false);

            View view = _customRenderer.Render(currentActivity, type, title, description, context);
            Crouton crouton = new Crouton(currentActivity, view, (int)duration.TotalMilliseconds, 
                b =>
                {
                    if (clickable)
                    {
                        taskCompletionSource.TrySetResult(b);
                    }
                }, context);
            crouton.Show();
            return taskCompletionSource.Task;
        }
        
        public Task NotifySticky(ToastNotificationType type, string title, string description, object context = null,
            bool clickable = true, CancellationToken cancellationToken = new CancellationToken(), bool modal = false)
        {
            throw new NotImplementedException("yet");
        }

        public void HideAll()
        {
            Manager.Instance.RemoveCroutons();
        }

        /// <summary>
        /// You can pass your custom renderer for toast layour, in case of null DefaultToastLayoutRenderer will be used
        /// </summary>
        public static void Init(Context context, IToastLayoutCustomRenderer customRenderer = null)
        {
            _context = context;
            _customRenderer = customRenderer ?? new DefaultToastLayoutRenderer();
        }
    }
}