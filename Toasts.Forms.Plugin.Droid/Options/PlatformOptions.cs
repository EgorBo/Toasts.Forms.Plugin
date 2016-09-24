namespace Plugin.Toasts
{
    public class PlatformOptions : IPlatformOptions
    {
        public int? SmallIconDrawable { get; set; } = null;

        public NotificationStyle Style { get; set; }
    }
}