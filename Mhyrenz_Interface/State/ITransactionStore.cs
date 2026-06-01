using System.Threading.Tasks;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.State
{
    public interface ITransactionStore : IViewModelStore<int, TransactionDataViewModel>
    {
        void AddToSale(CheckoutResult result);
        Task InitializeAsync();
    }
}