using System;
using System.Threading.Tasks;

namespace Plugin.Toasts
{
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
        Task<NotificationResult> Notify(INotificationOptions options);
    
    }
}