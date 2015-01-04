using System;

namespace Toasts.Forms.Plugin.Abstractions
{
    public interface IToastNotificator
    {
        void Show(ToastNotificationType type, string title, string description, TimeSpan duration, Action clickAction = null);
    }
}