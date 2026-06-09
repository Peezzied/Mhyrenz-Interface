using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Core.MVVM
{
    public interface ICommandAsync : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
