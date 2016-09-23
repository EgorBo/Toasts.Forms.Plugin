using System;
using System.Collections.Generic;
using System.Linq;

using Plugin.Toasts;
using Foundation;
using UIKit;
using Xamarin.Forms;
using UserNotifications;

namespace Toasts.Forms.Plugin.Sample.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Xamarin.Forms.Forms.Init();

            DependencyService.Register<ToastNotification>();
            ToastNotification.Init();

            LoadApplication(new App());

            // Request Permissions
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (granted, error) =>
            {
                // Do something if needed
            });

            return base.FinishedLaunching(app, options);
        }
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
           // Local Notifications are received here
        }
       
    }
}
