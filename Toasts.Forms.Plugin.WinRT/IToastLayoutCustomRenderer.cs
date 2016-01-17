using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Plugin.Toasts
{
    public interface IToastLayoutCustomRenderer
    {
        UIElement Render(ToastNotificationType type, string title, string description, object context, out Brush backgroundBrush);

        bool IsTappable { get; }

        bool HasCloseButton { get; }
    }
}
