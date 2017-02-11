namespace Plugin.Toasts
{
    using System.Collections.Generic;

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        bool IsClickable { get; }

        /// <summary>
        /// Will remove the notification from the notification center or tray after it has finished being displayed.
        /// </summary>
        bool ClearFromHistory { get; }

        IDictionary<string, string> CustomArgs { get; }

        IWindowsOptions WindowsOptions { get; }
        IAndroidOptions AndroidOptions { get; }
        IiOSOptions iOSOptions { get; }
    }
}
