namespace Plugin.Toasts
{
    using System;

    public class NotificationOptions : INotificationOptions
    {
        public string Description { get; set; } = string.Empty;

        public DateTime? Expiry { get; set; } = null;

        /// <summary>
        /// Only for Windows. It works in Android but it goes against Material Design Guidelines. iOS doesn't have option to override the logo url.
        /// </summary>
        public string LogoUri { get; set; } = null;

        public string Title { get; set; } = string.Empty;

        public bool IsClickable { get; set; } = false;
    }
}
