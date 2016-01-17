using System.Windows;
using System.Windows.Media;

namespace Plugin.Toasts
{
    public interface IToastLayoutCustomRenderer
    {
        UIElement Render(ToastNotificationType type, string title, string description, object context, out Brush backgroundBrush);

        bool IsTappable { get; }

        bool HasCloseButton { get; }
    }
}
