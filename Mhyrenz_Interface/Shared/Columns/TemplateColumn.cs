using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Shared.Columns
{
    public class TemplateColumn : DataGridTemplateColumn
    {
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (!cell.IsEditable(dataItem))
            {
                cell.IsEditing = false;
                return base.GenerateElement(cell, dataItem);
            }

            return base.GenerateEditingElement(cell, dataItem);
        }
    }
}
