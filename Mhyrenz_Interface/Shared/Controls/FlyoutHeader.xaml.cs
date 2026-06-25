using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Shared.Controls
{
    /// <summary>
    /// Interaction logic for FlyoutHeader.xaml
    /// </summary>
    public partial class FlyoutHeader : UserControl
    {


        public bool Shadow
        {
            get { return (bool)GetValue(ShadowProperty); }
            set { SetValue(ShadowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shadow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShadowProperty =
            DependencyProperty.Register(nameof(Shadow), typeof(bool), typeof(FlyoutHeader), new PropertyMetadata(true));

        public FlyoutHeader()
        {
            InitializeComponent();
        }
    }
}
