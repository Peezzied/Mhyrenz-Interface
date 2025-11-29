using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class UndoRedoBehavior : Behavior<Control>
    {
        private readonly static Dictionary<Control, DataGrid> dataGrid;
        private static BindingExpression bindingEx;

        static UndoRedoBehavior()
        {
            dataGrid = new Dictionary<Control, DataGrid>();
        }

        public DependencyProperty TargetProperty
        {
            get => (DependencyProperty)GetValue(TargetPropertyProperty);
            set => SetValue(TargetPropertyProperty, value);
        }

        public static readonly DependencyProperty TargetPropertyProperty =
            DependencyProperty.Register(
                nameof(TargetProperty),
                typeof(DependencyProperty),
                typeof(UndoRedoBehavior),
                new PropertyMetadata(null));

        public static void Renew()
        {
            dataGrid.Clear();
        }


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (dataGrid is null || !dataGrid.TryGetValue(AssociatedObject, out var dg))
                    dataGrid.Add(AssociatedObject, TreeHelper.TryFindParent<DataGrid>(AssociatedObject));

                bindingEx = AssociatedObject.GetBindingExpression(TargetProperty);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                TryUpdateSource(sender as DependencyObject, TargetProperty);
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void TryUpdateSource(DependencyObject target, DependencyProperty dp)
        {
            var expression = bindingEx;
            var control = target.CastTo<Control>();
            var viewModel = control.DataContext.CastTo<ProductDataViewModel>();
            var undoRedoManager = App.ServiceProvider.GetRequiredService<IUndoRedoManager>();
            var controlPropertyValue = control.GetValue(dp);

            var bindingPath = expression.ParentBinding.Path.Path;
            var match = Regex.Match(bindingPath, @"^(?<prop>\w+)\[(?<key>[^\]]+)\](?:\..*)?$");
            object vmPropertyValue;

            if (match.Success)
            {
                var prop = match.Groups["prop"].Value;
                var key = match.Groups["key"].Value;

                var dictObj = expression.DataItem.GetType().GetProperty(prop).GetValue(viewModel);
                var dictType = dictObj.GetType();

                var containsMethod = dictType.GetMethod("ContainsKey");
                bool exists = (bool)containsMethod.Invoke(dictObj, new object[] { key });

                var indexer = dictType.GetProperty("Item");
                vmPropertyValue = indexer.GetValue(dictObj, new object[] { key });
            }
            else
            {
                vmPropertyValue = expression.DataItem.GetType().GetProperty(expression.ResolvedSourcePropertyName).GetValue(viewModel);
            }


            if (dataGrid[AssociatedObject].DataContext.CastTo<InventoryDataGridViewModel>().IsEditCancelled)
                return;

            if (!undoRedoManager.CanRedo || controlPropertyValue.ToString() == vmPropertyValue.ToString())
            {
                expression?.UpdateSource();
                return;
            }

            object convertedValue;

            try
            {
                var targetType = Nullable.GetUnderlyingType(dp.PropertyType) ?? dp.PropertyType;

                if (vmPropertyValue == null || targetType.IsInstanceOfType(vmPropertyValue))
                {
                    convertedValue = vmPropertyValue;
                }
                else
                {
                    convertedValue = Convert.ChangeType(vmPropertyValue, targetType);
                }
            }
            catch
            {
                convertedValue = vmPropertyValue;
            }

            if (controlPropertyValue == vmPropertyValue)
            {
                control.SetValue(dp, convertedValue);
                return;
            }

            var prompt = undoRedoManager.ShowWarning(() =>
            {
                control.SetValue(dp, convertedValue);
            });

            if (prompt)
                expression?.UpdateSource();
        }
    }
}
