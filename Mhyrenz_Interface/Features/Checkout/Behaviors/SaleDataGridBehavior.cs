using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Features.Checkout.Commands;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Checkout.Behaviors
{
    public class SaleDataGridBehavior : Behavior<DataGrid>
    {
        public CheckoutViewModel Owner
        {
            get { return (CheckoutViewModel)GetValue(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
        }

        public static readonly DependencyProperty OwnerProperty =
            DependencyProperty.Register(nameof(Owner), typeof(CheckoutViewModel), typeof(SaleDataGridBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            Owner.RowIntoViewRequested += Owner_RowIntoViewRequested;
        }

        private async void Owner_RowIntoViewRequested(TransactionVMRowInfo info)
        {

            if (info.Sale != Owner.SelectedItem.Sale.Id)
            {
                Owner.SelectTab(info.Sale);
            }

            if (info.Transactions == null)
                return;

            var selectionSet = info.Transactions.ToHashSet();
            AssociatedObject.SelectedItems.Clear();

            foreach (var item in AssociatedObject.Items.Cast<TransactionDataViewModel>())
            {
                if (selectionSet.Contains(item.Transaction.Id))
                    AssociatedObject.SelectedItems.Add(item);
            }

            AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItems);
        }
    }
}
