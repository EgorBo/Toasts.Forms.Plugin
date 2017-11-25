namespace Plugin.Toasts
{
    public class NotificationResult : INotificationResult
    {
        public NotificationAction Action { get; set; }
        public int Id { get; set; }
    }
}
