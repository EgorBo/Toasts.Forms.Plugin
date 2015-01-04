using System;
using Android.App;
using Android.Widget;
using Toasts.Forms.Plugin.Abstractions;
using Toasts.Forms.Plugin.Droid;
using Xamarin.Forms;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

[assembly: Dependency(typeof(ToastNotificatorImplementation))]
namespace Toasts.Forms.Plugin.Droid
{
    public class ToastNotificatorImplementation : IToastNotificator
    {
        private static IToastLayoutCustomRenderer _customRenderer;

        public ToastNotificatorImplementation()
        {
        }

        public void Show(ToastNotificationType type, string title, string description, TimeSpan duration, Action clickAction = null)
        {
            Activity currentActivity = Xamarin.Forms.Forms.Context as Activity;
            if (currentActivity == null)
                return;

            View view;

            if (_customRenderer == null)
            {
                view = currentActivity.LayoutInflater.Inflate(Resource.Layout.crouton, null);

                var titleTv = view.FindViewById<TextView>(Resource.Id.title);
                var descTv = view.FindViewById<TextView>(Resource.Id.desc);
                var image = view.FindViewById<ImageView>(Resource.Id.image);

                titleTv.Text = title;
                descTv.Text = description;

                switch (type)
                {
                    case ToastNotificationType.Info:
                        image.SetImageResource(Resource.Drawable.info);
                        view.SetBackgroundColor(new Color(42, 112, 153));
                        break;
                    case ToastNotificationType.Success:
                        image.SetImageResource(Resource.Drawable.success);
                        view.SetBackgroundColor(new Color(69, 145, 34));
                        break;
                    case ToastNotificationType.Warning:
                        image.SetImageResource(Resource.Drawable.warning);
                        view.SetBackgroundColor(new Color(180, 125, 1));
                        break;
                    case ToastNotificationType.Error:
                        image.SetImageResource(Resource.Drawable.error);
                        view.SetBackgroundColor(new Color(206, 24, 24));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
            else
            {
                view = _customRenderer.Render(type, title, description);
            }

            Crouton crouton = new Crouton(currentActivity, view, (int)duration.TotalMilliseconds, clickAction);
            crouton.Show();
        }

        public static void Init(IToastLayoutCustomRenderer customRenderer = null)
        {
            _customRenderer = customRenderer;
        }
    }
}