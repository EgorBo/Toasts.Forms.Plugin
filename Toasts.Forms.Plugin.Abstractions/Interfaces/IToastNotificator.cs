namespace Plugin.Toasts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IToastNotificator
    {
        /// <summary>
        /// Show a Toast notification
        /// </summary>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        Task<INotificationResult> Notify(INotificationOptions options);

        /// <summary>
        /// Shows a Toast then runs the callback. Will not wait for the toast action to be completed but will call the callback instead when complete.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        void Notify(Action<INotificationResult> callback, INotificationOptions options);

        /// <summary>
        /// Delivered Notifications to the phone through the Toast Plugin
        /// UWP, iOS or Android >= API23 only.
        /// </summary>
        /// <returns></returns>
        Task<IList<INotification>> GetDeliveredNotifications();

        /// <summary>
        /// Cancels all currently showing or previously shown notifications
        /// </summary>
        void CancelAllDelivered();

        /// <summary>
        /// Used on some platforms to pass system args through on certain events
        /// </summary>
        /// <param name="args"></param>
        void SystemEvent(object args);

    }
}