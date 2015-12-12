using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;

namespace Toasts.Forms.Plugin.Sample.Droid
{
    [Activity(Label = "Toasts.Forms.Plugin.Sample", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init(Xamarin.Forms.Forms.Context);
            LoadApplication(new App());
        }
    }
}

