using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;

namespace Mhyrenz_Interface.Shared.Columns
{
    public class TemplateColumn: DataGridTemplateColumn
    {
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (!cell.IsEditable(((ProductDataViewModel)dataItem).Item))
            {
                cell.IsEditing = false;
                return base.GenerateElement(cell, dataItem);
            }

            return base.GenerateEditingElement(cell, dataItem);
        }
    }
}
