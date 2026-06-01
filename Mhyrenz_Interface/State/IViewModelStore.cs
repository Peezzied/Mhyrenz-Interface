using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.State
{
    public interface IViewModelStore<K, V>
    {
        SourceCollection<K, V> Store { get; }
    }
}