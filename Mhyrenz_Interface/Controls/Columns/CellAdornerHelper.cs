using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Controls.Behaviors;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Controls.Columns
{
    public static class CellAdornerHelper
    {
        public static FrameworkElement ApplyAdorner(FrameworkElement element, DependencyProperty property, object gridViewModel)
        {
            Interaction.GetBehaviors(element).Add(new UndoRedoBehavior
            {
                TargetProperty = property,
                ExpectedGridViewModel = gridViewModel,
                ExpectedRowViewModel = element.DataContext,
            });

            var adorner = new CellAdorner
            {
                Adorned = element
            };
            return adorner;
        }
    }
}