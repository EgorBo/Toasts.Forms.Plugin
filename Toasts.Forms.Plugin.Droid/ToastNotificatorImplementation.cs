using System;
using System.Threading.Tasks;
using Android.App;
using Toasts.Forms.Plugin.Abstractions;
using Toasts.Forms.Plugin.Droid;
using Xamarin.Forms;
using View = Android.Views.View;

[assembly: Dependency(typeof(ToastNotificatorImplementation))]
namespace Toasts.Forms.Plugin.Droid
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        public ToastNotificatorImplementation()
        {
        }

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Activity currentActivity = Xamarin.Forms.Forms.Context as Activity;
            if (currentActivity == null)
                return Task.FromResult(false);

            View view = _customRenderer.Render(currentActivity, type, title, description, context);
            Crouton crouton = new Crouton(currentActivity, view, (int)duration.TotalMilliseconds, b => taskCompletionSource.TrySetResult(b), context);
            crouton.Show();
            return taskCompletionSource.Task;
        }

        public void HideAll()
        {
            Manager.Instance.RemoveCroutons();
        }

        /// <summary>
        /// You can pass your custom renderer for toast layour, in case of null DefaultToastLayoutRenderer will be used
        /// </summary>
        public static void Init(IToastLayoutCustomRenderer customRenderer = null)
        {
            _customRenderer = customRenderer ?? new DefaultToastLayoutRenderer();
        }
    }
}