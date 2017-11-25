using Android.App;
using Android.OS;
using Plugin.Toasts;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Toasts.Forms.Plugin.Sample.DroidAppCompat
{
    [Activity(Label = "Toasts.Forms.Plugin.Sample.DroidAppCompat", Theme = "@style/MyTheme", MainLauncher = true, Icon = "@drawable/Icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            Xamarin.Forms.Forms.Init(this, bundle);

            DependencyService.Register<ToastNotification>();
            ToastNotification.Init(this, new PlatformOptions() { SmallIconDrawable = Android.Resource.Drawable.IcDialogInfo });

            LoadApplication(new App());
        }
    }
}

