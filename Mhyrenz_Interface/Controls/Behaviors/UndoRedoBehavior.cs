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
        private BindingExpression _bindingEx;

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

        public object ExpectedRowViewModel
        {
            get => GetValue(ExpectedRowViewModelProperty);
            set => SetValue(ExpectedRowViewModelProperty, value);
        }

        public static readonly DependencyProperty ExpectedRowViewModelProperty =
            DependencyProperty.Register(
                nameof(ExpectedRowViewModel),
                typeof(object),
                typeof(UndoRedoBehavior),
                new PropertyMetadata(null));

        public object ExpectedGridViewModel
        {
            get => GetValue(ExpectedGridViewModelProperty);
            set => SetValue(ExpectedGridViewModelProperty, value);
        }

        public static readonly DependencyProperty ExpectedGridViewModelProperty =
            DependencyProperty.Register(
                nameof(ExpectedGridViewModel),
                typeof(object),
                typeof(UndoRedoBehavior),
                new PropertyMetadata(null));

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
                _bindingEx = AssociatedObject.GetBindingExpression(TargetProperty);
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
            var expression = _bindingEx;
            if (expression == null)
                return;

            var control = target.CastTo<Control>();
            var viewModel = ExpectedRowViewModel ?? control.DataContext;
            var undoRedoManager = App.ServiceProvider.GetRequiredService<IUndoRedoManager>(); // FIXME: anti-pattern
            var controlPropertyValue = control.GetValue(dp);

            var bindingPath = expression.ParentBinding.Path.Path;
            var match = Regex.Match(bindingPath, @"^(?<prop>\w+)\[(?<key>[^\]]+)\](?:\..*)?$");

            object vmPropertyValue;

            if (match.Success)
            {
                var prop = match.Groups["prop"].Value;
                var key = match.Groups["key"].Value;

                var dictObj = viewModel.GetType()
                    .GetProperty(prop)
                    .GetValue(viewModel);

                var dictType = dictObj.GetType();

                var containsMethod = dictType.GetMethod("ContainsKey");
                bool exists = (bool)containsMethod.Invoke(dictObj, new object[] { key });

                if (!exists)
                    return;

                var indexer = dictType.GetProperty("Item");
                vmPropertyValue = indexer.GetValue(dictObj, new object[] { key });
            }
            else
            {
                vmPropertyValue = viewModel.GetType()
                    .GetProperty(expression.ResolvedSourcePropertyName)
                    .GetValue(viewModel);
            }

            if (ExpectedGridViewModel is IEditCancelState editState &&
                editState.IsEditCancelled)
            {
                return;
            }

            if (!undoRedoManager.CanRedo ||
                Equals(controlPropertyValue?.ToString(), vmPropertyValue?.ToString()))
            {
                expression.UpdateSource();
                return;
            }

            object convertedValue;

            try
            {
                var targetType =
                    Nullable.GetUnderlyingType(dp.PropertyType) ??
                    dp.PropertyType;

                convertedValue =
                    vmPropertyValue == null ||
                    targetType.IsInstanceOfType(vmPropertyValue)
                        ? vmPropertyValue
                        : Convert.ChangeType(vmPropertyValue, targetType);
            }
            catch
            {
                convertedValue = vmPropertyValue;
            }

            if (Equals(controlPropertyValue, vmPropertyValue))
            {
                control.SetValue(dp, convertedValue);
                return;
            }

            var prompt = undoRedoManager.ShowWarning(() =>
            {
                control.SetValue(dp, convertedValue);
            });

            if (prompt)
                expression.UpdateSource();
        }
    }
}
