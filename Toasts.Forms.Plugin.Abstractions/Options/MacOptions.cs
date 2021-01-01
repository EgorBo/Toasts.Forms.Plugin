namespace Plugin.Toasts
{
    public class MacOptions : IMacOptions
    {
        public int BadgeCount { get; set; } = 0;

        public bool SetBadgeCount { get; set; } = false;
    }
}