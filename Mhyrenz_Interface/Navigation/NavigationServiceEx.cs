using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Mhyrenz_Interface.Navigation
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(CancellationToken token);
    }

    public class NavigationServiceEx : INavigationServiceEx
    {
        private readonly NavigationViewModelFactory _viewModelFactory;
        private CancellationTokenSource _navigationCts;
        private int _navigationVersion;

        public NavigationViewModel CurrentViewModel { get; private set; }

        public event Action<NavigationViewModel, Type> Navigated;

        public NavigationServiceEx(NavigationViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public async Task<bool> NavigateAsync(Type viewType, PostNavigation postNavigationCallback = null)
        {
            var version = Interlocked.Increment(ref _navigationVersion);

            _navigationCts?.Cancel();
            _navigationCts?.Dispose();
            _navigationCts = new CancellationTokenSource();

            var token = _navigationCts.Token;

            try
            {
                if (CurrentViewModel?.GetType() == NavigationViewModelFactory.GetViewModelType(viewType))
                    return true;

                var oldVm = CurrentViewModel;

                CurrentViewModel = null;
                Navigated?.Invoke(null, viewType);

                if (oldVm is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else
                    oldVm?.Dispose();

                token.ThrowIfCancellationRequested();

                var newVm = _viewModelFactory.CreateViewModel(viewType);

                if (newVm is IAsyncInitializable asyncVm)
                    await asyncVm.InitializeAsync(token);

                token.ThrowIfCancellationRequested();

                if (token.IsCancellationRequested ||
                    version != _navigationVersion)
                {
                    newVm.Dispose();
                    return false;
                }

                CurrentViewModel = newVm;
                Navigated?.Invoke(CurrentViewModel, viewType);

                if (postNavigationCallback != null)
                {
                    await App.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        if (token.IsCancellationRequested ||
                            version != _navigationVersion)
                            return;

                        await postNavigationCallback(CurrentViewModel);
                    }, DispatcherPriority.Loaded);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }

        }
    }
}
