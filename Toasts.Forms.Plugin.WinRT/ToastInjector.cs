using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Plugin.Toasts
{
    internal static class ToastInjector
    {
        private static bool _injected = false;

        // TODO: I am using an async void, bad programmer
        public static async void Inject()
        {
            if (_injected)
                return;

            _injected = true;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                // Let's inject our toast into frame using a special Frame template defined in FrameStyle.xaml
                var frameStyleRd = new ResourceDictionary();
                var frame = (Frame)Window.Current.Content;
                frameStyleRd.Source = new Uri("ms-appx:///FrameStyle.xaml",
                    UriKind.Absolute);
                Application.Current.Resources.MergedDictionaries.Add(frameStyleRd);
                frame.Style = Application.Current.Resources["MainFrameStyle"] as Style;
            });
        }
    }
}
