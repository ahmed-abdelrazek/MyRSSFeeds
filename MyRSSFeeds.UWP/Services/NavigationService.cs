﻿using MyRSSFeeds.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MyRSSFeeds.Services
{
    public static class NavigationService
    {
        public static event NavigatedEventHandler Navigated;

        public static event EventHandler<bool> OnCurrentPageCanGoBackChanged;

        public static event NavigationFailedEventHandler NavigationFailed;

        private static Frame _frame;
        private static object _lastParamUsed;
        private static bool _canCurrentPageGoBack;

        public static Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
                    RegisterFrameEvents();
                }

                return _frame;
            }

            set
            {
                UnregisterFrameEvents();
                _frame = value;
                RegisterFrameEvents();
            }
        }

        public static bool CanGoBack => Frame.CanGoBack;

        public static bool CanGoForward => Frame.CanGoForward;

        public static bool GoBack()
        {
            if (_canCurrentPageGoBack)
            {
                if (Frame.Content is FrameworkElement element && element.DataContext is IBackNavigationHandler navigationHandler)
                {
                    navigationHandler.GoBack();
                    return true;
                }
            }

            if (CanGoBack)
            {
                Frame.GoBack();
                return true;
            }

            return false;
        }

        public static void GoForward() => Frame.GoForward();

        public static bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(pageType, parameter, infoOverride);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                }

                return navigationResult;
            }
            else
            {
                return false;
            }
        }

        public static bool Navigate<T>(object parameter = null, NavigationTransitionInfo infoOverride = null)
            where T : Page
            => Navigate(typeof(T), parameter, infoOverride);

        private static void RegisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated += Frame_Navigated;
                _frame.Navigating += Frame_Navigating;
                _frame.NavigationFailed += Frame_NavigationFailed;
            }
        }

        private static void UnregisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated -= Frame_Navigated;
                _frame.Navigating -= Frame_Navigating;
                _frame.NavigationFailed -= Frame_NavigationFailed;
            }
        }

        private static void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => NavigationFailed?.Invoke(sender, e);

        private static void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (Frame.Content is FrameworkElement element && element.DataContext is IBackNavigationHandler backNavigationHandler)
            {
                backNavigationHandler.OnPageCanGoBackChanged += OnPageCanGoBackChanged;
            }

            Navigated?.Invoke(sender, e);
        }

        private static void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Frame.Content is FrameworkElement element && element.DataContext is IBackNavigationHandler backNavigationHandler)
            {
                backNavigationHandler.OnPageCanGoBackChanged -= OnPageCanGoBackChanged;
                _canCurrentPageGoBack = false;
            }
        }

        private static void OnPageCanGoBackChanged(object sender, bool canCurrentPageGoBack)
        {
            _canCurrentPageGoBack = canCurrentPageGoBack;
            OnCurrentPageCanGoBackChanged?.Invoke(sender, canCurrentPageGoBack);
        }
    }
}
