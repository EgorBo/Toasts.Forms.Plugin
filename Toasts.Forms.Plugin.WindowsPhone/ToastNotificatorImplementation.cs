using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Toasts.Forms.Plugin.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(Toasts.Forms.Plugin.WindowsPhone.ToastNotificatorImplementation))]
namespace Toasts.Forms.Plugin.WindowsPhone
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        /// <summary>
        /// Should be called after Xamarin.Forms.Init();
        /// </summary>
        /// <param name="stackSize">max toast messages count - not implemented on ios and android yet - they show only 1 toast max</param>
        /// <param name="customRenderer">you can override default layout by passing custom renderer</param>
        public static void Init(int stackSize = 3, IToastLayoutCustomRenderer customRenderer = null)
        {
            ToastPromtsHostControl.MaxToastCount = stackSize;
            _customRenderer = customRenderer;
            ToastInjector.Inject();
        }

        public void Show(ToastNotificationType type, string title, string description, TimeSpan duration, Action clickAction = null)
        {
            UIElement element;
            Brush brush;
            bool hasCloseButton = false;
            bool tappable = true;

            if (_customRenderer == null)
            {
                string iconFile;

                switch (type)
                {
                    case ToastNotificationType.Info:
                        brush = new SolidColorBrush(Color.FromArgb(255, 42, 112, 153));
                        iconFile = "info.png";
                        break;
                    case ToastNotificationType.Success:
                        brush = new SolidColorBrush(Color.FromArgb(255, 69, 145, 34));
                        iconFile = "success.png";
                        break;
                    case ToastNotificationType.Warning:
                        brush = new SolidColorBrush(Color.FromArgb(255, 180, 125, 1));
                        iconFile = "warning.png";
                        break;
                    case ToastNotificationType.Error:
                        brush = new SolidColorBrush(Color.FromArgb(255, 206, 24, 24));
                        iconFile = "error.png";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }

                var titleTb = new TextBlock();
                titleTb.Text = title;
                titleTb.VerticalAlignment = VerticalAlignment.Center;
                titleTb.Margin = new Thickness(12, 6, 0, 0);
                titleTb.TextTrimming = TextTrimming.None;
                titleTb.TextWrapping = TextWrapping.NoWrap;
                titleTb.Foreground = new SolidColorBrush(Colors.White);
                titleTb.FontSize = 22;
                titleTb.FontWeight = FontWeights.Bold;
                Grid.SetColumn(titleTb, 1);
                Grid.SetRow(titleTb, 0);

                var descTb = new TextBlock();
                descTb.Text = description;
                descTb.VerticalAlignment = VerticalAlignment.Center;
                descTb.Margin = new Thickness(12, -4, 0, 0);
                descTb.TextTrimming = TextTrimming.WordEllipsis;
                descTb.TextWrapping = TextWrapping.NoWrap;
                descTb.Foreground = new SolidColorBrush(Colors.White);
                descTb.FontSize = 18;
                Grid.SetColumn(descTb, 1);
                Grid.SetRow(descTb, 1);

                Image image = new Image();
                image.Width = 42;
                image.Height = 42;
                image.Margin = new Thickness(10, 0, 0, 0);
                image.VerticalAlignment = VerticalAlignment.Center;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.Source = LoadBitmapImage(iconFile);
                Grid.SetRowSpan(image, 2);

                Grid layout = new Grid();
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                layout.ColumnDefinitions.Add(new ColumnDefinition());
                layout.RowDefinitions.Add(new RowDefinition());
                layout.RowDefinitions.Add(new RowDefinition());

                layout.Children.Add(image);
                layout.Children.Add(titleTb);
                layout.Children.Add(descTb);

                element = layout;
            }
            else
            {
                element = _customRenderer.Render(type, title, description, out brush);
                tappable = _customRenderer.IsTappable;
                hasCloseButton = _customRenderer.HasCloseButton;
            }

            if (clickAction == null)
                clickAction = delegate { };

            ToastPromtsHostControl.EnqueueItem(element, b => clickAction(), brush, tappable: tappable, timeout: duration, showCloseButton: hasCloseButton);
        }

        public static BitmapImage LoadBitmapImage(string fileName)
        {
            Uri imgUri = new Uri("/Toasts.Forms.Plugin.WindowsPhone;component/Icons/" + fileName, UriKind.Relative);
            StreamResourceInfo imageResource = Application.GetResourceStream(imgUri);
            BitmapImage image = new BitmapImage();
            image.SetSource(imageResource.Stream);
            return image;
        }
    }
}
