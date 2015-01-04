using System;
using System.Threading.Tasks;
using Toasts.Forms.Plugin.Abstractions;
using Toasts.Forms.Plugin.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastNotificatorImplementation))]
namespace Toasts.Forms.Plugin.iOS
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static MessageBarStyleSheet _customStyle;

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            MessageBarManager.SharedInstance.ShowMessage(title, description, type, b => taskCompletionSource.TrySetResult(b), duration, _customStyle);
            return taskCompletionSource.Task;
        }

        public void HideAll()
        {
            MessageBarManager.SharedInstance.HideAll();
        }

        public static void Init(MessageBarStyleSheet customStyle = null)
        {
            _customStyle = customStyle ?? new MessageBarStyleSheet();
        }
    }
}