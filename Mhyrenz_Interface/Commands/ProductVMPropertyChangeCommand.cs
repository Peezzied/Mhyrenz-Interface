using System;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMRowInfo
    {
        public int Category { get; set; }
        public int[] Products { get; set; }
    }

    public abstract class ProductVMPropertyChangeCommand : PropertyChangeCommand<ProductVMRowInfo>
    {
        public ProductVMPropertyChangeCommand(ChangedArgs args, TrackPropertyHelper.Setter setter, Action propertyChangeHandler, Type currentViewIn) : 
            base(args, setter, propertyChangeHandler, currentViewIn)
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