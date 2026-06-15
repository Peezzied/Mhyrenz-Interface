using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Mhyrenz_Interface.Features.Home.Controls
{
    public class ActionButton : Button
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ActionButton), new PropertyMetadata(null));

        public PackIconMaterialLightKind IconKind
        {
            get => (PackIconMaterialLightKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind),
                typeof(PackIconMaterialLightKind),
                typeof(ActionButton),
                new PropertyMetadata(PackIconMaterialLightKind.None));
    }
}
