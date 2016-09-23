Toasts Notification Plugin for Xamarin and Windows
===================

## Currently under a rebuild.
Due to the recent advancedments in notification systems on each platform, I am bring it into the future. Using native toast / notification platform instead of custom UI overlays.

This is a sharp detour but brings the UI inline with user expectations and doesn't try to shove a similar look and experience across platforms.

A simple way of showing some notifications inside your Xamarin or Windows application. In windows phone world we call them "Toasts".

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


In your iOS, Android, and UWP projects please call:

```csharp
DependencyService.Register<ToastNotification>(); // Register your dependency

// If you are using Android you must also call Init
ToastNotification.Init(this);
```

If you are using Xamarin Forms, you must do this AFTER your call to Xamarin.Forms.Init();

#### Usage
Use dependency service in order to resolve IToastNotificator
```csharp
var notificator = DependencyService.Get<IToastNotificator>();
bool tapped = await notificator.Notify(ToastNotificationType.Error, 
	"Error", "Something went wrong", TimeSpan.FromSeconds(2));
```
#### Customization
On all three platforms you can completely override toast UI. However, there is also an easy way to add a new status with a custom icon and background, let's take a look on a Windows Phone example. In this example we want to add a few more types of messages with custom icons. Our code will look like this:
```csharp
var notificator = DependencyService.Get<IToastNotificator>();
bool tapped = await notificator.Notify(
	type: ToastNotificationType.Custom, 
	title: "Level up!", 
	description: "Congratulations!", 
	duration: TimeSpan.FromSeconds(2), 
	context: MyCustomTypes.LevelUp);
```			
So we have just set the type to Custom and passed an additional argument to Object context (last argument) - our custom enum MyCustomTypes.LevelUp.
Now we have to configure default renderer (or replace it by your own if you want to add more changes to the layout):
```csharp
Xamarin.Forms.Forms.Init();
ToastNotificatorImplementation.Init(stackSize: 2, 
	customRenderer: new DefaultToastLayoutRenderer(
		context =>
		{
			switch ((MyCustomTypes)context)
			{
				case MyCustomTypes.LevelUp:
					return new BitmapImage(new Uri("level_up.png", UriKind.Relative));
				...
			}
		}, 
		context => new SolidColorBrush(Colors.Magenta)));
```
iOS and Android api also have similar extensibility

#### Contributors
* [EgorBo](https://github.com/EgorBo)
* [AdamPed](https://github.com/AdamPed)

Thanks!

#### License
Licensed under MIT
