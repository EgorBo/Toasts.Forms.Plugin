using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Toasts
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static MessageBarStyleSheet _customStyle;

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context, bool clickable = true)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            MessageBarManager.SharedInstance.ShowMessage(title, description, type, b =>
                {
                    if (clickable)
                    {
                        taskCompletionSource.TrySetResult(b);
                    }
                }, duration, _customStyle);
            return taskCompletionSource.Task;
        }

        public Task NotifySticky(ToastNotificationType type, string title, string description, object context = null,
            bool clickable = true, CancellationToken cancellationToken = new CancellationToken(), bool modal = false)
        {
            throw new NotImplementedException("yet");
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