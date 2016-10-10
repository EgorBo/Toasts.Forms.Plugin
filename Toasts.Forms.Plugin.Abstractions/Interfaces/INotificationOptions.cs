namespace Plugin.Toasts
{
    using System.Collections.Generic;

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        bool IsClickable { get; }
        IDictionary<string, string> CustomArgs { get; }

        IWindowsOptions WindowsOptions { get; }
        IAndroidOptions AndroidOptions { get; }
        IiOSOptions iOSOptions { get; }
    }
}
