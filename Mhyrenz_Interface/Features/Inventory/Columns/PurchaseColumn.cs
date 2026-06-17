using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HandyControl.Controls;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Columns;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Inventory.Columns
{
    public class PurchaseColumn: NumberColumn
    {

        private BindingBase _valueBinding;
        public BindingBase ValueBinding
        {
            get => _valueBinding;
            set => _valueBinding = value;
        }

        private BindingBase _maximumBinding;
        public BindingBase MaximumBinding
        {
            get => _maximumBinding;
            set => _maximumBinding = value;
        }

        private BindingBase _isRightClickedBinding;
        public BindingBase IsRightClickedBinding
        {
            get => _isRightClickedBinding;
            set => _isRightClickedBinding = value;
        }

        private BindingBase _rightClickValueBinding;
        public BindingBase RightClickValueBinding
        {
            get => _rightClickValueBinding;
            set => _rightClickValueBinding = value;
        }

        private BindingBase _rightClickMaximumBinding;
        public BindingBase RightClickMaximumBinding
        {
            get => _rightClickMaximumBinding;
            set => _rightClickMaximumBinding = value;
        }

        private BindingBase _canEditBinding;
        public BindingBase CanEditBinding
        {
            get => _canEditBinding;
            set => _canEditBinding = value;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (dataItem is ProductDataViewModel vm && vm.HasActiveSale)
            {
                MessageBox.Show(
                    "This product cannot be modified or deleted because it is currently part of an active sale.\n\n" +
                    "Please complete or remove the item from sale before continuing.",
                    "Action Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                cell.IsEditing = false;
                return base.GenerateElement(cell, dataItem);
            }

            return base.GenerateEditingElement(cell, dataItem);
        }
    }
}
