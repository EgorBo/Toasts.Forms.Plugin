using System;
using Xamarin.Forms;
using Plugin.Toasts;

namespace Toasts.Forms.Plugin.Sample
{
    public class App : Application
    {
        public App()
        {
            Button showInfo = new Button { Text = "Info" };
            showInfo.Clicked += (s, e) => ShowToast(ToastNotificationType.Info);
           
            Button showSuccess = new Button { Text = "Success" };
            showSuccess.Clicked += (s, e) => ShowToast(ToastNotificationType.Success);

            Button showWarning = new Button { Text = "Warning" };
            showWarning.Clicked += (s, e) => ShowToast(ToastNotificationType.Warning);

            Button showError = new Button { Text = "Error" };
            showError.Clicked += (s, e) => ShowToast(ToastNotificationType.Error);

            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        showInfo,
                        showSuccess,
                        showWarning,
                        showError
					}
                }
            };
            

        }

        private async void ShowToast(ToastNotificationType type)
        {
            var notificator = DependencyService.Get<IToastNotificator>();
            bool tapped = await notificator.Notify(type, "Some " + type.ToString().ToLower(), "Some description", TimeSpan.FromSeconds(2));
        }
    }
}
