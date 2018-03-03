namespace Plugin.Toasts
{
    using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;

    public class NotificationOptions : INotificationOptions
    {
        public string Description { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool IsClickable { get; set; } = false;
        
        public IWindowsOptions WindowsOptions { get; set; } = new WindowsOptions();

        public IAndroidOptions AndroidOptions { get; set; } = new AndroidOptions();

        public IiOSOptions iOSOptions { get; set; } = new iOSOptions();

        public IDictionary<string, string> CustomArgs { get; set; } = new ConcurrentDictionary<string, string>();

        public bool ClearFromHistory { get; set; } = false;

        public DateTime? DelayUntil { get; set; } = null;

        public bool AllowTapInNotificationCenter { get; set; } = false;
    }
}
