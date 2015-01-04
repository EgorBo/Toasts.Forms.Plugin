using System;
using Toasts.Forms.Plugin.Abstractions;
using Toasts.Forms.Plugin.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastNotificatorImplementation))]
namespace Toasts.Forms.Plugin.iOS
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static MessageBarStyleSheet _customStyle;

        public void Show(ToastNotificationType type, string title, string description, TimeSpan duration, Action clickAction = null)
        {
            MessageBarManager.SharedInstance.ShowMessage(title, description, type, clickAction, duration, _customStyle);
        }

        public static void Init(MessageBarStyleSheet customStyle = null)
        {
            _customStyle = customStyle;
        }
    }
}