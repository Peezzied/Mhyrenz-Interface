using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMRowInfo
    {
        public int Category { get; set; }
        public int[] Products { get; set; }
    }

    public abstract class ProductVMPropertyChangeCommand : PropertyChangeCommand<ProductVMRowInfo>
    {
        public ProductVMPropertyChangeCommand(DTO dto) : base(dto)
        {
            SideEffect = SideEffectHandler;
        }

        private void SideEffectHandler(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;
            view.RowIntoView(PropertyChangedArgs.RowInfo.Category, PropertyChangedArgs.RowInfo.Products);
        }
    }
}