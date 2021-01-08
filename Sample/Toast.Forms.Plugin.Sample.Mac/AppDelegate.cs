using System;
using AppKit;
using Foundation;
using UserNotifications;
using Xamarin.Forms.Platform.MacOS;
using Plugin.Toasts;
using Toasts.Forms.Plugin.Sample;

namespace Toast.Forms.Plugin.Sample.Mac
{
    [Register ("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public AppDelegate ()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect (250, 50, 720, 500);
            window = new NSWindow (rect, style, NSBackingStore.Buffered, false) {
                Title = "Toasts.Forms.Plugins", // choose your own Title here
                TitleVisibility = NSWindowTitleVisibility.Hidden
            };

            // Add Quit shortcut
            NSMenu appMenubar = new NSMenu ();
            NSMenuItem appMenuItem = new NSMenuItem ();
            appMenubar.AddItem (appMenuItem);

            NSMenu appMenu = new NSMenu ();
            appMenuItem.Submenu = appMenu;

            var quitMenuItem = new NSMenuItem ("Quit", "q", delegate {
                NSApplication.SharedApplication.Terminate (appMenubar);
            });
            appMenu.AddItem (quitMenuItem);

            NSApplication.SharedApplication.MainMenu = appMenubar;
        }

        NSWindow window;
        public override NSWindow MainWindow => window;
        private UNUserNotificationCenter currentNotificationCenter;

        public override void WillFinishLaunching (NSNotification notification)
        {
            // Check we're at least v10.14
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion (new NSOperatingSystemVersion (10, 14, 0))) {
                currentNotificationCenter = UNUserNotificationCenter.Current;
                currentNotificationCenter.Delegate = new UNUserNotificationCenterDelegate ();
                // Request Permissions
                currentNotificationCenter.RequestAuthorization (UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (granted, error) => {
                    if (error != null) {
                        NSApplication.SharedApplication.InvokeOnMainThread (() => {
                            var alert = new NSAlert {
                                InformativeText = "Notification Request Error",
                                MessageText = error.LocalizedDescription + Environment.NewLine + "Reason: " + error.LocalizedFailureReason,
                                AlertStyle = NSAlertStyle.Critical,
                            };
                            alert.AddButton ("Ok");

                            alert.RunModal ();
                        });
                    }
                });
            }
        }

        public override void DidFinishLaunching (NSNotification notification)
        {
            Xamarin.Forms.Forms.Init ();

            Xamarin.Forms.DependencyService.Register<ToastNotification> (); // Register your dependency
            ToastNotification.Init ();

            LoadApplication (new App ());

            base.DidFinishLaunching (notification);
        }

        public override void WillTerminate (NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}