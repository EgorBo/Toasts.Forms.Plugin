namespace Plugin.Toasts
{
    public interface INotificationResult
    {
        NotificationAction Action { get; set; }
        int Id { get; set; }
    }
}
