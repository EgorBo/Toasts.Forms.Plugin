using Android.App;
using Android.Views;
using Toasts.Forms.Plugin.Abstractions;

namespace Toasts.Forms.Plugin.Droid
{
    public interface IToastLayoutCustomRenderer
    {
        View Render(Activity activity, ToastNotificationType type, string title, string description, object context);
    }
}
