namespace Plugin.Toasts
{
    public class iOSOptions : IiOSOptions
    {
        public int BadgeCount { get; set; } = 0;

        public bool SetBadgeCount { get; set; } = false;
    }
}
