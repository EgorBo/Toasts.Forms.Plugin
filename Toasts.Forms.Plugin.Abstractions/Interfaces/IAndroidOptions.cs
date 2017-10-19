using Plugin.Toasts.Interfaces;
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
        string HexColour { get; set; }

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
        IAndroidChannelOptions ChannelOptions { get; set; }

        //
        // Debug Help
        //

        /// <summary>
        /// Displays a toast with the Id and Result Action whenever a local notification callback is triggered.
        /// Useful with DebugShowIdInTitle for testing and debugging.
        /// </summary>
        /// <value><c>true</c> if show debug callback toast; otherwise, <c>false</c>.</value>
        bool DebugShowCallbackToast { get; set; }

        /// <summary>
        /// Displays the internal Id of the notification at the beginning of the Notification title.
        /// Useful with DebugShowCallbackToast for testing and debugging.
        /// </summary>
        /// <value><c>true</c> if debug show identifier in title; otherwise, <c>false</c>.</value>
        bool DebugShowIdInTitle { get; set; }
    }
}
