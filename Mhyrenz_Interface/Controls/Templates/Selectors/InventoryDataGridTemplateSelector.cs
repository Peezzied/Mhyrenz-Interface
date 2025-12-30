using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Controls.Templates.Selectors
{
    public class InventoryDataGridTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DetailedTemplate { get; set; }
        public DataTemplate CompactedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is InventoryDataGridViewModel vm)
            {
                switch (vm.Layout)
                {
                    case InventoryDataGridLayout.Compacted: return CompactedTemplate;
                    case InventoryDataGridLayout.Detailed: return DetailedTemplate;
                    default:
                        break;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
