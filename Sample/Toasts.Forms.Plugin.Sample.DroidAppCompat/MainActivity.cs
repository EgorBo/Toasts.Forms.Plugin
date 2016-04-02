using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Plugin.Toasts;

namespace Toasts.Forms.Plugin.Sample.DroidAppCompat
{
    [Activity(Label = "Toasts.Forms.Plugin.Sample.DroidAppCompat", Theme= "@style/MyTheme", MainLauncher = true, Icon = "@drawable/Icon")]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;
            FormsAppCompatActivity.TabLayoutResource = Resource.Layout.tabs;

            Xamarin.Forms.Forms.Init(this, bundle);

            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init(this);

            LoadApplication(new App());
        }
    }
}

