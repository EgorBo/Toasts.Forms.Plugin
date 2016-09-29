namespace Plugin.Toasts
{
    using System;

    public class Notification : INotification
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Delivered { get; set; }
    }
}
