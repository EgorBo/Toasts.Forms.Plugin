namespace Plugin.Toasts
{
    using System;

    public class NotificationOptions : INotificationOptions
    {
        public string Description { get; set; } = string.Empty;

        public DateTime? Expiry { get; set; } = null;

        public Uri LogoSource { get; set; } = null;

        public string Title { get; set; } = string.Empty;
    }
}
