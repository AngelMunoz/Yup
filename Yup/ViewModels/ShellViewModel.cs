﻿using System;
using System.Linq;
using Caliburn.Micro;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Yup.Core.Models;
using Yup.Helpers;
using Yup.Views;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Yup.ViewModels
{
  public class ShellViewModel : Screen, IHandle<NavigationArgs>
  {
    private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
    private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);
    private readonly IEventAggregator _ea;
    private readonly WinRTContainer _container;
    private static INavigationService _navigationService;
    private WinUI.NavigationView _navigationView;
    private bool _isBackEnabled;
    private WinUI.NavigationViewItem _selected;

    public ShellViewModel(WinRTContainer container, IEventAggregator ea)
    {
      _container = container;
      _ea = ea;
      _ea.Subscribe(this);
    }

    public bool IsBackEnabled
    {
      get { return _isBackEnabled; }
      set { Set(ref _isBackEnabled, value); }
    }

    public WinUI.NavigationViewItem Selected
    {
      get { return _selected; }
      set { Set(ref _selected, value); }
    }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      var view = GetView() as IShellView;

      _navigationService = view?.CreateNavigationService(_container);
      _navigationView = view?.GetNavigationView();

      if (_navigationService != null)
      {
        _navigationService.NavigationFailed += (sender, e) =>
        {
          throw e.Exception;
        };
        _navigationService.Navigated += NavigationService_Navigated;
        _navigationView.BackRequested += OnBackRequested;
      }
    }

    protected override void OnViewLoaded(object view)
    {
      base.OnViewLoaded(view);
      if (GetView() is UIElement page)
      {
        page.KeyboardAccelerators.Add(_altLeftKeyboardAccelerator);
        page.KeyboardAccelerators.Add(_backKeyboardAccelerator);
      }
    }

    private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
    {
      if (args.IsSettingsInvoked)
      {
        _navigationService.Navigate(typeof(SettingsPage));
        return;
      }

      var item = _navigationView.MenuItems
                      .OfType<WinUI.NavigationViewItem>()
                      .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
      var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
      var viewModelType = ViewModelLocator.LocateTypeForViewType(pageType, false);
      _navigationService.NavigateToViewModel(viewModelType);
    }

    private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
    {
      _navigationService.GoBack();
    }

    private void NavigationService_Navigated(object sender, NavigationEventArgs e)
    {
      IsBackEnabled = _navigationService.CanGoBack;
      if (e.SourcePageType == typeof(SettingsPage))
      {
        Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
        return;
      }

      Selected = _navigationView.MenuItems
                      .OfType<WinUI.NavigationViewItem>()
                      .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
    }

    private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
    {
      var sourceViewModelType = ViewModelLocator.LocateTypeForViewType(sourcePageType, false);
      var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
      var viewModelType = ViewModelLocator.LocateTypeForViewType(pageType, false);
      return viewModelType == sourceViewModelType;
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
      var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
      if (modifiers.HasValue)
      {
        keyboardAccelerator.Modifiers = modifiers.Value;
      }

      keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
      return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
      if (_navigationService.CanGoBack)
      {
        _navigationService.GoBack();
        args.Handled = true;
      }
    }

    public void Handle(NavigationArgs message)
    {
      var item = _navigationView
        .MenuItems
        .OfType<WinUI.NavigationViewItem>()
        .First(menuItem => (string)menuItem.Content == message.ViewName);
      var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
      var viewModelType = ViewModelLocator.LocateTypeForViewType(pageType, false);
      _navigationService.NavigateToViewModel(viewModelType, message.Url);
    }
  }
}
