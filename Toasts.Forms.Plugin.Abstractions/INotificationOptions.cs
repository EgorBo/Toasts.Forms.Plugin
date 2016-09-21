namespace Plugin.Toasts
{
    using System;

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        DateTime? Expiry { get; }
        Uri LogoSource { get; }
    }
}
