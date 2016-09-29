namespace Plugin.Toasts
{
    using System;
    using System.Collections.Generic;

    public interface INotification
    {
        string Id { get; set; }
        string Title { get; }
        string Description { get; }

        /// <summary>
        /// When the notification was delivered in UTC
        /// </summary>
        DateTime Delivered { get; }
    }
}
