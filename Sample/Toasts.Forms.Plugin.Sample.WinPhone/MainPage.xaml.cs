using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Xamarin.Forms;
using Plugin.Toasts;

namespace Toasts.Forms.Plugin.Sample.WinPhone
{
    public partial class MainPage : global::Xamarin.Forms.Platform.WinPhone.FormsApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            Xamarin.Forms.Forms.Init();

            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init(stackSize: 2);

            LoadApplication(new Sample.App());
        }
    }
}
