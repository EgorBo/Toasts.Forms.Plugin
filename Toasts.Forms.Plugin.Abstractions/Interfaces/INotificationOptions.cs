namespace Plugin.Toasts
{
    using System;
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
        /// <summary>
        /// Schedules the notification in the future. Note: When set the NotificationResult returned will always be NotApplicable and will be returned immediately.
        /// </summary>
        DateTime? DelayUntil { get; }

        IDictionary<string, string> CustomArgs { get; }

        IWindowsOptions WindowsOptions { get; }
        IAndroidOptions AndroidOptions { get; }
        IiOSOptions iOSOptions { get; }
        bool AllowTapInNotificationCenter { get; }
        
    }
}
