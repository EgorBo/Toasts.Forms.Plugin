using Plugin.Toasts.Options;

namespace Plugin.Toasts
{
    public interface IAndroidOptions
    {
        /// <summary>
        /// Applicable only to Notification.Builder, if you want to replace the small icon, you must place an image in your drawables folder and pass the int through here. e.g. Resources.Drawable.MyNewIcon
        /// </summary>
        int? SmallDrawableIcon { get; set; }
        string ViewText { get; set; }
        string DismissText { get; set; }

        /// <summary>
        /// Gets or sets the hex colour for the notification to use.
        /// Colour usage changes based on the Android version.
        /// Example: #FF00CC
        /// </summary>
        /// <value>The hex colour</value>
        string HexColor { get; set; }

        /// <summary>
        /// Will attempt to bring the app to the foreground, or launch the app after tapping a local notification.
        /// </summary>
        /// <value><c>true</c> if force open app on notification tap; otherwise, <c>false</c>.</value>
        bool ForceOpenAppOnNotificationTap { get; set; }

        /// <summary>
        /// Gets or sets the notification channel options.
        /// Android Oreo (26) and above only.
        /// </summary>
        /// <value>The notification channel options.</value>
        AndroidChannelOptions ChannelOptions { get; set; }
    }
}
