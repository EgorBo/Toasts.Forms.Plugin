namespace Toasts
{
    public enum ToastNotificationType
    {
        Info,
        Success,
        Warning,
        Error,

        /// <summary>
        /// NOTE: for this icon you should implement your own icon resolver or custom renderer
        /// </summary>
        Custom
    }
}
