namespace Plugin.Toasts
{
    using System;

    [Flags]
    public enum NotificationAction
    {
        Timeout = 1, // Hides by itself
        Clicked = 2, // User clicked on notification
        Dismissed = 4, // User manually dismissed notification
        ApplicationHidden = 8, // Application went to background
        Failed = 16, // When failed to display the toast,
        NotApplicable = 32 // When the ability to determine the outcome is not available
    }
}
