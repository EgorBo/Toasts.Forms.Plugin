using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Plugin.Toasts
{
    internal static class ToastInjector
    {
        private static bool _injected = false;

        public static void Inject()
        {
            if (_injected)
                return;

            _injected = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Let's inject our toast into frame using a special Frame template defined in FrameStyle.xaml
                    var frameStyleRd = new ResourceDictionary();
                    var frame = (PhoneApplicationFrame)Application.Current.RootVisual;
                    frameStyleRd.Source = new Uri("/Toasts.Forms.Plugin.WindowsPhone;component/FrameStyle.xaml",
                        UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(frameStyleRd);
                    frame.Style = Application.Current.Resources["MainFrameStyle"] as Style;
                });
        }
    }
}
