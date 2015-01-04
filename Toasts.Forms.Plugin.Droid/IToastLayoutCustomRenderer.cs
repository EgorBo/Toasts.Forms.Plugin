using Android.Views;
using Toasts.Forms.Plugin.Abstractions;

namespace Toasts.Forms.Plugin.Droid
{
    public interface IToastLayoutCustomRenderer
    {
        View Render(ToastNotificationType type, string title, string description);
    }
}
