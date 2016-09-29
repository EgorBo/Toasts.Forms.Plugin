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
    }
}
