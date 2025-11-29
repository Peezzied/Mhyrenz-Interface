using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.ViewModels
{
    public class TransactionViewModel : NavigationViewModel
    {
        public TransactionViewModel(INavigationServiceEx navigationServiceEx) : base(navigationServiceEx)
        {
        }
    }
}
