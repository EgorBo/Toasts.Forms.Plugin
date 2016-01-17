using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Plugin.Toasts
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        /// <summary>
        /// Should be called after Xamarin.Forms.Init();
        /// </summary>
        /// <param name="stackSize">max toast messages count - not implemented on ios and android yet - they show only 1 toast max</param>
        /// <param name="customRenderer">you can override default layout by passing custom renderer, null means DefaultToastLayoutRenderer</param>
        public static void Init(int stackSize = 3, IToastLayoutCustomRenderer customRenderer = null)
        {
            ToastPromptsHostControl.MaxToastCount = stackSize;
            _customRenderer = customRenderer ?? new DefaultToastLayoutRenderer();
            ToastInjector.Inject();
        }

        public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context, bool clickable = true)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Brush brush;
            var element = _customRenderer.Render(type, title, description, context, out brush);

            ToastPromptsHostControl.EnqueueItem(element, b =>
                {
                    if (clickable)
                    {
                        taskCompletionSource.TrySetResult(b);
                    }
                }, brush, 
                tappable: _customRenderer.IsTappable, 
                timeout: duration, 
                showCloseButton: _customRenderer.HasCloseButton);
            return taskCompletionSource.Task;
        }

        public Task NotifySticky(ToastNotificationType type, string title, string description, object context = null,
            bool clickable = true, CancellationToken cancellationToken = default (CancellationToken), bool modal = false)
        {
            throw new NotImplementedException("yet");
        }

        public void HideAll()
        {
            ToastPromptsHostControl.Clear();
        }
    }
}
