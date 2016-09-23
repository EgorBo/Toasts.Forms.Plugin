Toasts Notification Plugin for Xamarin and Windows
===================

A simple way of showing notifications inside your Xamarin or Windows application. In windows phone world we call them "Toasts". This plugin uses the platforms native toast / notification API's.

Setup and usage
===================
#### Setup
* Available on NuGet: https://www.nuget.org/packages/Toasts.Forms.Plugin
* Install into your PCL project and Client projects.

**Platform Support**

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|No||
|Xamarin.iOS Unified (v10 SDK+)|Yes|iOS 7+|
|Xamarin.Android|Yes|API 16+ (AppCompat Only)|
|Windows Phone Silverlight|No||
|Windows Phone RT|Yes|8.1+|
|Windows Store RT|Yes|8.1+|
|Windows 10 UWP|Yes|10+|
|Xamarin.Mac|No||

In your iOS, Android, WinRT and UWP projects please call:

```csharp
DependencyService.Register<ToastNotification>(); // Register your dependency
ToastNotification.Init();

// If you are using Android you must pass through the activity
ToastNotification.Init(this);
```

If you are using Xamarin Forms, you must do this AFTER your call to Xamarin.Forms.Init();

#### Usage
Use dependency service in order to resolve IToastNotificator
```csharp
var notificator = DependencyService.Get<IToastNotificator>();

var options = new NotificationOptions()
            {
                Title = "Title",
                Description = "Description"
            };

var result = await notificator.Notify(options);
```

The result that is returned is one of the following options
```csharp
[Flags]
public enum NotificationResult
{
    Timeout = 1, // Hides by itself
    Clicked = 2, // User clicked on notification
    Dismissed = 4, // User manually dismissed notification
    ApplicationHidden = 8, // Application went to background
    Failed = 16 // When failed to display the toast
}
```

If you want the Clicked `NotificationResult` you must set `IsClickable = true` in the `NotificationOptions`.

#### Permissions

In iOS you must request permission to show local notifications first since it is a user interrupting action.

```csharp
// Request Permissions
UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (granted, error) =>
{
     // Do something if needed
});
```

#### Contributors
* [EgorBo](https://github.com/EgorBo)
* [AdamPed](https://github.com/AdamPed)

Thanks!

#### License
Licensed under MIT
