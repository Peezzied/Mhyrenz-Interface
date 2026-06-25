using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMRowInfo : IRowInfo
    {
        public int Category { get; set; }
        public int[] Products { get; set; }
    }

    public abstract class ProductVMPropertyChangeCommand : PropertyChangeCommand<ProductVMRowInfo>
    {
        public ProductVMPropertyChangeCommand(DTO dto) : base(dto, typeof(InventoryView))
        {
            Completer = CompleterHandler;
        }

        protected virtual async Task CompleterHandler(BaseViewModel vm)
        {
            var view = vm as InventoryViewModel;
            view.RowIntoView(PropertyChangedArgs.RowInfo);
        }
    }
}