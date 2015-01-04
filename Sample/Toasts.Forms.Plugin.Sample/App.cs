using System;
using System.Diagnostics;
using Toasts.Forms.Plugin.Abstractions;
using Xamarin.Forms;

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

        private void ShowToast(ToastNotificationType type)
        {
            var notificator = DependencyService.Get<IToastNotificator>();
            notificator.Show(type, "Some " + type.ToString().ToLower(), "Some description", TimeSpan.FromSeconds(2), () => OnToastClick(type));
        }

        private void OnToastClick(ToastNotificationType type)
        {
            Debug.WriteLine("Toast {0} is clicked");
        }
    }
}
