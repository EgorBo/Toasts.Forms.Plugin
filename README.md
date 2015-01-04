Toasts Plugin for Xamarin.Forms
===================

A simple way of showing some notifications inside your Xamarin.Forms application. In windows phone world we call them "Toasts".

#### Android
Android implementation is based on https://github.com/keyboardsurfer/Crouton with several changes and ported to C# without bindings.

![Alt text](http://habrastorage.org/files/b72/3c4/362/b723c436271941309939da500f1e2abb.gif)

#### iOS
iOS implementation is based on https://github.com/terryworona/TWMessageBarManager with several changes (thanks to prashantvc with his https://github.com/prashantvc/Xamarin.iOS-MessageBar)

![Alt text](http://habrastorage.org/files/d1e/dd7/cbd/d1edd7cbdfe141cfb8f7be36f692b1a1.gif)

#### WP8
Unlike others WP8's version supports multiply toasts (can be limited to 1).

![Alt text](http://habrastorage.org/files/e96/4fd/8c5/e964fd8c5cb14ad08d4dab3cb6f36e73.gif)

Setup and usage
===================
#### Setup
* Available on NuGet: https://www.nuget.org/packages/Toasts.Forms.Plugin
* Install into your PCL project and Client projects.

In your iOS, Android, and Windows Phone projects please call:

```
Xamarin.Forms.Init();//platform specific init
ToastNotificatorImplementation.Init(); //you can pass your own 
                                       //custom renderer as an argument here
```

You must do this AFTER you call Xamarin.Forms.Init();

#### Usage
Use dependency service in order to resolve IToastNotificator
```
var notificator = DependencyService.Get<IToastNotificator>();
bool tapped = await notificator.Notify(ToastNotificationType.Error, 
	"Error", "Something went wrong", TimeSpan.FromSeconds(2));
```


#### Contributors
* [EgorBo](https://github.com/EgorBo)

Thanks!

#### License
Licensed under MIT