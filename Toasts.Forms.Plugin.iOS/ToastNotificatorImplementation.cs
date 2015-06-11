using System;
using System.Threading.Tasks;
using Toasts;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastNotificatorImplementation))]
namespace Toasts
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static MessageBarStyleSheet _customStyle;

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context)
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