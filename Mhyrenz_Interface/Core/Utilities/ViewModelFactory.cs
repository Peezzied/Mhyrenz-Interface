using Mhyrenz_Interface.Core.MVVM;

namespace Mhyrenz_Interface.Core.Utilities
{
    public class ViewModelFactory<T> : IViewModelFactory<T> where T : BaseViewModel
    {
        private readonly CreateViewModel<T> _vm;

        public ViewModelFactory(CreateViewModel<T> entity)
        {
            _vm = entity;
        }

        public T CreateViewModel(object parameter = null)
        {
            return _vm(parameter);
        }
    }
}
