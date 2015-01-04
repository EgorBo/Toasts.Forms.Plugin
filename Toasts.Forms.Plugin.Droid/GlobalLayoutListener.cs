using System;
using Android.Views;

namespace Toasts.Forms.Plugin.Droid
{
    internal class GlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        public void OnGlobalLayout(Action action)
        {
            action();
        }

        public void OnGlobalLayout() {}
    }
}