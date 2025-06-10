using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels.Factory
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
