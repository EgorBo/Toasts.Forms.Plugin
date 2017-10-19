using System;
using Plugin.Toasts.Interfaces;
using Plugin.Toasts.Options;

namespace Plugin.Toasts
{
    public class AndroidOptions : IAndroidOptions
    {
        /// <summary>
        /// Applicable only to Notification.Builder, if you want to replace the small icon, you must place an image in your drawables folder and pass the int through here. e.g. Resources.Drawable.MyNewIcon
        /// </summary>
        public int? SmallDrawableIcon { get; set; } = null;
        public string DismissText { get; set; } = "Dismiss";
        public string ViewText { get; set; } = "View";
        public string HexColour { get; set; } = "#FFFFFFFF";
        public bool ForceOpenAppOnNotificationTap { get; set; } = false;
        public IAndroidChannelOptions ChannelOptions { get; set; } = new AndroidChannelOptions();

        // Debug Help
        public bool DebugShowCallbackToast { get; set; } = false;
        public bool DebugShowIdInTitle { get; set; } = false;
    }
}
