using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace Mhyrenz_Interface.Shared.Controls
{
    public class MenuItem : HamburgerMenuIconItem
    {

        public static readonly DependencyProperty NavigationTypeProperty = DependencyProperty.Register(
          nameof(NavigationType), typeof(Type), typeof(MenuItem), new PropertyMetadata(default(Type)));

        public Type NavigationType
        {
            get => (Type)this.GetValue(NavigationTypeProperty);
            set => this.SetValue(NavigationTypeProperty, value);
        }
    }
}
