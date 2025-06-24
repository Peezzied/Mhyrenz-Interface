using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Mhyrenz_Interface.Navigation
{
    public class NavigationServiceEx: INavigationServiceEx
    {
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                _currentViewModel?.Dispose();

                _currentViewModel = value;
            }
        }
        public event EventHandler Navigating;

        public event NavigatedEventHandler Navigated;

        public event NavigationFailedEventHandler NavigationFailed;
        public event Action TransitionCompleted;

        private Frame _frame;

        public Frame Frame
        {
            get
            {
                if (this._frame == null)
                {
                    this._frame = new Frame() { NavigationUIVisibility = NavigationUIVisibility.Hidden };
                    this.RegisterFrameEvents();
                }

                return this._frame;
            }
            set
            {
                this.UnregisterFrameEvents();
                this._frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public bool CanGoForward => this.Frame.CanGoForward;


        public void GoForward() => this.Frame.GoForward();
        public void GoBack()
        {
            this.Frame.GoBack();
        }

        public bool Navigate(Uri sourcePageUri, object extraData = null)
        {
            if (this.Frame.CurrentSource != sourcePageUri)
            {
                //Debug.WriteLine($"Navigating to {sourcePageUri} with extra data: {extraData}");
                var result = this.Frame.Navigate(sourcePageUri, extraData);

                if (result)
                    this.Frame.Navigated += SetDataContextAfterNavigation;

                return result;
            }

            return false;
        }

        public bool Navigate(Type sourceType)
        {
            if (this.Frame.NavigationService?.Content?.GetType() != sourceType)
            {
                //Debug.WriteLine($"Navigating to {sourceType}");
                var result = this.Frame.Navigate(Activator.CreateInstance(sourceType));

                if (result)
                    this.Frame.Navigated += SetDataContextAfterNavigation;
            }

            return false;
        }

        private void RegisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.LostFocus += (s, e) => Navigating?.Invoke(s, EventArgs.Empty);
                this._frame.Navigated += this.Frame_Navigated;
                this._frame.NavigationFailed += this.Frame_NavigationFailed;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.LostFocus -= (s, e) => Navigating?.Invoke(s, EventArgs.Empty);
                this._frame.Navigated -= this.Frame_Navigated;
                this._frame.NavigationFailed -= this.Frame_NavigationFailed;
            }
        }

        private void SetDataContextAfterNavigation(object sender, NavigationEventArgs e)
        {
            if (e.Content is FrameworkElement element && CurrentViewModel != null)
            {
                element.DataContext = CurrentViewModel;
            }

            // Unsubscribe after setting it once
            Frame.Navigated -= SetDataContextAfterNavigation;
        }


        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => this.NavigationFailed?.Invoke(sender, e);

        private void Frame_Navigated(object sender, NavigationEventArgs e) => this.Navigated?.Invoke(sender, e);

        public void TransitionComplete()
        {
            TransitionCompleted?.Invoke();
        }
    }
}
