using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Yup.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yup.Views
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class DatabasesPage : Page
  {
    public DatabasesPage()
    {
      InitializeComponent();
      Loaded += DatabasesPage_Loaded;
      Unloaded += DatabasesPage_Unloaded;
    }

    public DatabasesViewModel ViewModel
    {
      get => DataContext as DatabasesViewModel;
    }

    private void DatabasesPage_Unloaded(object sender, RoutedEventArgs e)
    {
      ViewModel.OnEntrySelected -= ViewModel_OnEntrySelected;
    }

    private void DatabasesPage_Loaded(object sender, RoutedEventArgs e)
    {
      ViewModel.OnEntrySelected += ViewModel_OnEntrySelected;
      EditorWebView.Source = new Uri("ms-appx-web:///Html/MonacoEditor.html");
    }

    private async void ViewModel_OnEntrySelected(object sender, Core.Models.DatabaseEntry e)
    {
      var contents = JsonConvert.SerializeObject(e);
      var response = await EditorWebView.InvokeScriptAsync("setEditorContent", new string[] { contents });
      if (response.Length > 0)
      {
        Debug.WriteLine($"{response[0]} - {response[1]}");
      }
      ViewModel.IsLoadingEditor = false;
    }

    private async void Execute_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
      await ViewModel.OnExecuteStatement();
    }
  }
}
