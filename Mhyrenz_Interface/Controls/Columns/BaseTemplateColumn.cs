using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Controls.Behaviors;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Controls.Columns
{
    public abstract class BaseTemplateColumn : DataGridTemplateColumn
    {
        [Category("Common")]
        [Localizability(LocalizationCategory.None)]
        [Description("The path to bind to. Must be set.")]
        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(NumberColumn), new PropertyMetadata(null));

        protected abstract (FrameworkElement Element, DependencyProperty Property) EditingElement();
        protected abstract FrameworkElement Element();

        protected sealed override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var control = EditingElement();
            Interaction.GetBehaviors(control.Element).Add(new UndoRedoBehavior
            {
                TargetProperty = control.Property
            });

            var adorner = new CellAdorner
            {
                Adorned = control.Element
            };
            return adorner;
        }
        protected sealed override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return Element();
        }
    }
}