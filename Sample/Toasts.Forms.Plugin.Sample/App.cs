using Xamarin.Forms;
using Plugin.Toasts;
using System;
using Plugin.Toasts.Options;

namespace Toasts.Forms.Plugin.Sample
{
    public class App : Application
    {
        public App()
        {
            Button showToast = new Button { Text = "Show Toast" };

            showToast.Clicked += (s, e) =>
            {
                for (int i = 0; i < 6; i++)
                {
                    ShowToast(new NotificationOptions()
                    {
                        Title = "The Title Line",
                        Description = "The Description Content",
                        IsClickable = true,
                        WindowsOptions = new WindowsOptions() { LogoUri = "icon.png" },
                        ClearFromHistory = false,
                        //DelayUntil = DateTime.Now.AddSeconds((new Random()).Next((3 + i), (6 + i))),
                        AndroidOptions = new AndroidOptions()
                        {
                            HexColour = "#F99D1C",
                            ForceOpenAppOnNotificationTap = true
                        }
                    });
                }
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
            notificator.Notify((INotificationResult result) =>
            {
                System.Diagnostics.Debug.WriteLine("Notification [" + result.Id + "] Result Action: " + result.Action);
            }, options);
        }

    }
}
