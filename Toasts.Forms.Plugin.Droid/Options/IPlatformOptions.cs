namespace Plugin.Toasts
{
    public interface IPlatformOptions
    {
        NotificationStyle Style { get; }

        /// <summary>
        /// The default drawable icon for the small icon on the notification, using Notification.Buidler. Not applicable to Snackbar
        /// </summary>
        int? SmallIconDrawable { get; }
    }
}