using System.Windows;
using System.Windows.Media;
using Toasts.Forms.Plugin.Abstractions;

namespace Toasts.Forms.Plugin.WindowsPhone
{
    public interface IToastLayoutCustomRenderer
    {
        UIElement Render(ToastNotificationType type, string title, string description, object context, out Brush backgroundBrush);

        bool IsTappable { get; }

        bool HasCloseButton { get; }
    }
}
