using Xamarin.Forms;
using Plugin.Toasts;
using System;
using Plugin.Toasts.Options;
using System.Threading.Tasks;

namespace Toasts.Forms.Plugin.Sample
{
    public class App : Application
    {
        public App()
        {
            Button showToast = new Button { Text = "Show Toast" };

            showToast.Clicked += (s, e) =>
            {

                ShowToast(new NotificationOptions()
                {
                    Title = "The Title Line",
                    Description = "The Description Content",
                    IsClickable = true,
                    WindowsOptions = new WindowsOptions() { LogoUri = "icon.png" },
                    ClearFromHistory = false,
                    AllowTapInNotificationCenter = false,
                    AndroidOptions = new AndroidOptions()
                    {
                        HexColor = "#F99D1C",
                        ForceOpenAppOnNotificationTap = true
                    }
                });
            };

            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        showToast
                    }
                }
            };

        }

        void ShowToast(INotificationOptions options)
        {
            var notificator = DependencyService.Get<IToastNotificator>();

           // await notificator.Notify(options);

            notificator.Notify((INotificationResult result) =>
            {
                System.Diagnostics.Debug.WriteLine("Notification [" + result.Id + "] Result Action: " + result.Action);
            }, options);
        }

    }
}
