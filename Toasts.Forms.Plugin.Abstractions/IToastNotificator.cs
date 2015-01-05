using System;
using System.Threading.Tasks;

namespace Toasts.Forms.Plugin.Abstractions
{
    public interface IToastNotificator
    {
        /// <returns>true means clicked, false means displayed and then disapeared</returns>
        Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null);

        /// <summary>
        /// Clear notifications queue
        /// </summary>
        void HideAll();
    }
}