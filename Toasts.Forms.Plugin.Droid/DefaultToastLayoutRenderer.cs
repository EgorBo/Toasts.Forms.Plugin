using System;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Runtime;

namespace Plugin.Toasts
{
    public class DefaultToastLayoutRenderer : IToastLayoutCustomRenderer
    {
        private readonly Action<object, ImageView> _imageSetterForCustomType;
        private readonly Func<object, Color> _backgroundColorForCustomTypeResolver;

        public DefaultToastLayoutRenderer(Action<object, ImageView> imageSetterForCustomType = null, Func<object, Color> backgroundColorForCustomTypeResolver = null)
        {
            _imageSetterForCustomType = imageSetterForCustomType;
            _backgroundColorForCustomTypeResolver = backgroundColorForCustomTypeResolver;
        }

        public virtual View Render(Activity activity, ToastNotificationType type, string title, string description, object context)
        {
            var view = activity.LayoutInflater.Inflate(Resource.Layout.crouton, null);

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
                case ToastNotificationType.Custom:
                    _imageSetterForCustomType(context, image);
                    view.SetBackgroundColor(_backgroundColorForCustomTypeResolver(context));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            // First, we get application metrics
            IWindowManager windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            var metrics = new Android.Util.DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);
    
            // then we convert statusbar height in dpi to real pixels
            int statusBarDpiHeight = 24;
            int sizeInPixels = Convert.ToInt32(Math.Ceiling(statusBarDpiHeight * metrics.Density));
    
            // and least we set the Y of the view to be just under the statusbar
            view.SetY(sizeInPixels);
      
            return view;
        }
    }
}
