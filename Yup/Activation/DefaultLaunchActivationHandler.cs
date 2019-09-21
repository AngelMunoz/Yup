﻿using System;
using System.Threading.Tasks;

using Caliburn.Micro;

using Windows.ApplicationModel.Activation;

namespace Yup.Activation
{
  internal class DefaultLaunchActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
  {
    private readonly Type _navElement;
    private readonly INavigationService _navigationService;

    public DefaultLaunchActivationHandler(Type navElement, INavigationService navigationService)
    {
      _navElement = navElement;
      _navigationService = navigationService;
    }

    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
      // When the navigation stack isn't restored navigate to the first page,
      // configuring the new page by passing required information as a navigation
      // parameter
      _navigationService.NavigateToViewModel(_navElement, args.Arguments);

      // TODO WTS: Remove or change this sample which shows a toast notification when the app is launched.
      // You can use this sample to create toast notifications where needed in your app.
      //Singleton<ToastNotificationsService>.Instance.ShowToastNotificationSample();
      await Task.CompletedTask;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
      // None of the ActivationHandlers has handled the app activation
      return _navigationService.SourcePageType == null && _navElement != null;
    }

  }
}