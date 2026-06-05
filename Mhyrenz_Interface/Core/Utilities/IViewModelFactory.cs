namespace Mhyrenz_Interface.Core.Utilities
{
    public interface IViewModelFactory<T>
    {
        T CreateViewModel(object parameter = null);
    }
}