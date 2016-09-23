namespace Plugin.Toasts
{

    public interface INotificationOptions
    {
        string Title { get; }
        string Description { get; }
        string LogoUri { get; }
        bool IsClickable { get; }
    }
}
