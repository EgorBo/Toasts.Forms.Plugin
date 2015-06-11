using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toasts
{
    public interface IToastNotificator
    {
        /// <returns>true means clicked, false means displayed and then disapeared</returns>
        Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null, bool clickable = true);

        /// <summary>
        /// Shows toast until Cancel is requested (CancellationToken) or tapped (if clickable is set to true)
        /// </summary>
        Task NotifySticky(ToastNotificationType type, string title, string description, object context = null, bool clickable = true, CancellationToken cancellationToken = default(CancellationToken), bool modal = false);

        /// <summary>
        /// Clear notifications queue
        /// </summary>
        void HideAll();
    }
}