namespace Plugin.Toasts
{
    using System;

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        DateTime? Expiry { get; }
        string LogoUri { get; }
        bool IsClickable { get; }
    }
}
