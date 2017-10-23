using System;
using Plugin.Toasts.Interfaces;
namespace Plugin.Toasts.Options
{
    public class AndroidChannelOptions : IAndroidChannelOptions
    {
        public string Name { get; set; } = "default";
        public string Description { get; set; } = null;
        public bool EnableVibration { get; set; } = true;
        public bool ShowBadge { get; set; } = true;

        public AndroidChannelOptions() { }

        public AndroidChannelOptions(string name, string description = null, bool enableVibration = true, bool showBadge = true)
        {
            Name = name;
            Description = description;
            EnableVibration = enableVibration;
            ShowBadge = showBadge;
        }
    }
}
