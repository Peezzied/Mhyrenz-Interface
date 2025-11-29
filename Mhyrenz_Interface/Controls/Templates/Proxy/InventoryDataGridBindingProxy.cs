using System.Windows;

namespace Mhyrenz_Interface.Controls.Templates.Proxy
{
    public class InventoryDataGridBindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new InventoryDataGridBindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object),
                                         typeof(InventoryDataGridBindingProxy));
    }
}
