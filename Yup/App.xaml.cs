using System;
using System.Collections.Generic;

using Caliburn.Micro;


using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Yup.Core.Services;
using Yup.Services;
using Yup.ViewModels;

namespace Yup
{
  [Windows.UI.Xaml.Data.Bindable]
  public sealed partial class App
  {
    private readonly Lazy<ActivationService> _activationService;

    private ActivationService ActivationService
    {
      get { return _activationService.Value; }
    }

    public App()
    {
      InitializeComponent();
      Initialize();

      // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
      _activationService = new Lazy<ActivationService>(CreateActivationService);
    }
    private void ExtendAcrylicIntoTitleBar()
    {
      CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
      var titleBar = ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ButtonBackgroundColor = Colors.Transparent;
      titleBar.ButtonInactiveBackgroundColor = Colors.LightSlateGray;
      titleBar.BackgroundColor = Colors.Transparent;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
      if (!args.PrelaunchActivated)
      {
        await ActivationService.ActivateAsync(args);
        ExtendAcrylicIntoTitleBar();
      }
    }

    protected override async void OnActivated(IActivatedEventArgs args)
    {
      await ActivationService.ActivateAsync(args);
    }

    private WinRTContainer _container;

    protected override void Configure()
    {
      // This configures the framework to map between MainViewModel and MainPage
      // Normally it would map between MainPageViewModel and MainPage
      var config = new TypeMappingConfiguration
      {
        IncludeViewSuffixInViewModelNames = false
      };

      ViewLocator.ConfigureTypeMappings(config);
      ViewModelLocator.ConfigureTypeMappings(config);

      _container = new WinRTContainer();
      _container.RegisterWinRTServices();
      _container.RegisterSingleton(typeof(MongoService), "MongoService", typeof(MongoService));
      _container.RegisterPerRequest(typeof(PreviousConnectionsService), "PreviousConnectionsService", typeof(PreviousConnectionsService));

      _container.PerRequest<ShellViewModel>();
      _container.PerRequest<MainViewModel>();
      _container.PerRequest<DatabasesViewModel>();
      _container.PerRequest<SettingsViewModel>();
    }

    protected override object GetInstance(Type service, string key)
    {
      return _container.GetInstance(service, key);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
      return _container.GetAllInstances(service);
    }

    protected override void BuildUp(object instance)
    {
      _container.BuildUp(instance);
    }

    private ActivationService CreateActivationService()
    {
      return new ActivationService(_container, typeof(ViewModels.MainViewModel), new Lazy<UIElement>(CreateShell));
    }

    private UIElement CreateShell()
    {
      var shellPage = new Views.ShellPage();
      return shellPage;
    }
  }
}
