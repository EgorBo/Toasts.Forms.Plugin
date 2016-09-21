using Android.App;
using System;
using System.Threading.Tasks;
using View = Android.Views.View;

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
      
        public void HideAll()
        {
            Manager.Instance.RemoveCroutons();
        }
               
        /// <summary>
        /// You can pass your custom renderer for toast layout, in case of null DefaultToastLayoutRenderer will be used
        /// </summary>
        /// <param name="activity">The current activity. In Xamarin Forms pass the instance of the MainActity e.g. Init(this);</param>
        /// <param name="customRenderer"></param>
        public static void Init(IToastLayoutCustomRenderer customRenderer = null)
        {
            _activity = (Activity)Application.Context;
            _customRenderer = customRenderer ?? new DefaultToastLayoutRenderer();
        }
    }
}