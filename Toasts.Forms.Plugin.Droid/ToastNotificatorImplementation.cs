using System;
using System.Threading.Tasks;
using Android.App;
using View = Android.Views.View;
using System.Threading;
using Android.Content;

namespace Plugin.Toasts
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        private static Activity _activity = null;
        public ToastNotificatorImplementation()
        {
            
        }

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null, bool clickable = true)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            
            if (_activity == null)
                return Task.FromResult(false);

            View view = _customRenderer.Render(_activity, type, title, description, context);

            Crouton crouton = new Crouton(_activity, view, (int)duration.TotalMilliseconds, 
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
        /// You can pass your custom renderer for toast layout, in case of null DefaultToastLayoutRenderer will be used
        /// </summary>
        /// <param name="activity">The current activity. In Xamarin Forms pass the instance of the MainActity e.g. Init(this);</param>
        /// <param name="customRenderer"></param>
        public static void Init(Activity activity, IToastLayoutCustomRenderer customRenderer = null)
        {
            _activity = activity;
            _customRenderer = customRenderer ?? new DefaultToastLayoutRenderer();
        }
    }
}