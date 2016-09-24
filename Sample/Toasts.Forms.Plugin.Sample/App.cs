using Xamarin.Forms;
using Plugin.Toasts;

namespace Toasts.Forms.Plugin.Sample
{
    public class App : Application
    {
        public App()
        {

            var options = new NotificationOptions()
            {
                Title = "Title",
                Description = "Some Description",
                IsClickable = true,
                WindowsOptions = new WindowsOptions() { LogoUri = "icon.png" }
            };

            Button showToast = new Button { Text = "Show Toast" };
            showToast.Clicked += (s, e) => ShowToast(options);
            
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

        private async void ShowToast(INotificationOptions options)
        {
            var notificator = DependencyService.Get<IToastNotificator>();
            var result = await notificator.Notify(options);
        }
    }
}
