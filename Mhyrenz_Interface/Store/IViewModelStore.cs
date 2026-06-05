using Mhyrenz_Interface.Core.Collection;

namespace Mhyrenz_Interface.Store
{
    public interface IViewModelStore<K, V>
    {
        SourceCollection<K, V> Store { get; }
    }
}