using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;

namespace Mhyrenz_Interface.ViewModels
{
    public class SaleTabItem : BaseViewModel
    {
        public SaleTabItem(string header, InventoryDataGridViewModel viewModel)
        {
            Header = header;
            ContentViewModel = viewModel;
            Sales = new ObservableCollection<SaleViewModel>();

            SaleDropHandler = new SaleDropHandler();
            InventoryDragHandler = new InventoryDragHandler(this);
        }

        public string Header { get; set; }

        public ObservableCollection<SaleViewModel> Sales { get; }

        private DataGridRowDetailsVisibilityMode _productRowDetailsVisibilityMode =
            DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        public DataGridRowDetailsVisibilityMode ProductRowDetailsVisibilityMode
        {
            get => _productRowDetailsVisibilityMode;
            set
            {
                _productRowDetailsVisibilityMode = value;
                OnPropertyChanged(nameof(ProductRowDetailsVisibilityMode));
            }
        }

        public InventoryDataGridViewModel ContentViewModel { get; }

        public SaleDropHandler SaleDropHandler { get; }
        public InventoryDragHandler InventoryDragHandler { get; }
    }

    public class InventoryDragHandler : DefaultDragHandler
    {
        public InventoryDragHandler(SaleTabItem saleTabItem)
        {
            SaleTabItem = saleTabItem;
        }

        public SaleTabItem SaleTabItem { get; }

        public override void StartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is ProductDataViewModel product)
            {
                SaleTabItem.ProductRowDetailsVisibilityMode =
                    DataGridRowDetailsVisibilityMode.Collapsed;

                dragInfo.Data = product;
                dragInfo.Effects = DragDropEffects.Copy;
            }
        }

        public override void DragDropOperationFinished(
            DragDropEffects operationResult,
            IDragInfo dragInfo)
        {
            SaleTabItem.ProductRowDetailsVisibilityMode =
                DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }
    }

    public class SaleDropHandler : DefaultDropHandler
    {
        public override void DragOver(IDropInfo dropInfo)
        {
            base.DragOver(dropInfo);
            if (dropInfo.Data is ProductDataViewModel)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = null;
            }
        }

        public override void Drop(IDropInfo dropInfo)
        {
            // TODO: DROP LOGIC
            //if (dropInfo.Data is ProductDataViewModel product &&
            //    dropInfo.TargetCollection is ObservableCollection<SaleItemViewModel> saleItems)
            //{
            //    saleItems.Add(new SaleItemViewModel
            //    {
            //        ProductId = product.Item.Id,
            //        Name = product.Name,
            //        Price = product.RetailPrice,
            //        Quantity = 1
            //    });
            //}
        }
    }
}