using System.Windows;
using Mhyrenz_Interface.Shared.Behaviors;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Shared.Adorners
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