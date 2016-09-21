using Toasts.Forms.Plugin.WinRT;
using Xamarin.Forms;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Toasts.Forms.Plugin.Sample.WinRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            
            DependencyService.Register<ToastNotification>();
  
            LoadApplication(new Sample.App());
        }

     
    }
}
