
using Windows.UI.Xaml.Controls;
using Yup.ViewModels;

namespace Yup.Views
{
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      InitializeComponent();
    }

    private MainViewModel ViewModel
    {
      get { return DataContext as MainViewModel; }
    }

    private void Navigate_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
      ViewModel.OnConnect();
    }
  }
}
