namespace Plugin.Toasts
{

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        bool IsClickable { get; }

        IWindowsOptions WindowsOptions { get; }
        IAndroidOptions AndroidOptions { get; }
        IiOSOptions iOSOptions { get; }
    }
}
