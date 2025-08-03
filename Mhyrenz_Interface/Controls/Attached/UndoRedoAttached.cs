using HandyControl.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using Control = System.Windows.Controls.Control;
using MessageBox = HandyControl.Controls.MessageBox;
using NumericUpDown = MahApps.Metro.Controls.NumericUpDown;
using TextBox = System.Windows.Controls.TextBox;

namespace Mhyrenz_Interface.Controls.Attached
{
    public static class UndoRedoAttached
    {
        public static readonly DependencyProperty UndoRedoBoundProperty =
            DependencyProperty.RegisterAttached(
                "UndoRedoBound",
                typeof(bool),
                typeof(UndoRedoAttached),
                new PropertyMetadata(false, OnUndoRedoBound));

        public static void SetUndoRedoBound(DependencyObject element, bool value)
            => element.SetValue(UndoRedoBoundProperty, value);

        public static bool GetUndoRedoBound(DependencyObject element)
            => (bool)element.GetValue(UndoRedoBoundProperty);

        private static readonly ConditionalWeakTable<Control, Delegate> _controlEvents = new ConditionalWeakTable<Control, Delegate>();

        private static void OnUndoRedoBound(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) return;
            var control = d as Control;

            RoutedEventHandler unloadedHandler = null;

            switch (d)
            {
                case TextBox textBox:
                    unloadedHandler = TrackControl(textBox, TextBox.TextProperty, textBox.Text);
                    break;
                case NumericUpDown numericUpDown:
                    unloadedHandler = TrackControl(numericUpDown, NumericUpDown.ValueProperty, numericUpDown.Value);
                    break;
            }

            if (unloadedHandler != null)
            {
                control.Unloaded += unloadedHandler;

                void CleanupUnloaded(object sender, RoutedEventArgs args)
                {
                    control.Unloaded -= unloadedHandler;
                }

                control.Unloaded += CleanupUnloaded;
            }
        }

        private static RoutedEventHandler TrackControl(Control textBox, DependencyProperty dp, object value)
        {
            RoutedEventHandler unloadedHandler;
            RoutedEventHandler handler = (s, _) =>
                TryUpdateSource(textBox, dp, value);

            textBox.LostFocus += handler;
            _controlEvents.Add(textBox, handler);

            unloadedHandler = (_, __) =>
            {
                if (_controlEvents.TryGetValue(textBox, out var h))
                {
                    textBox.LostFocus -= h as RoutedEventHandler;
                    _controlEvents.Remove(textBox);
                }
            };
            return unloadedHandler;
        }

        private static void TryUpdateSource(DependencyObject target, DependencyProperty dp, object value)
        {
            var expression = BindingOperations.GetBindingExpression(target, dp);
            var control = target.CastTo<Control>();
            var viewModel = control.DataContext.CastTo<ProductDataViewModel>();
            var undoRedoManager = App.ServiceProvider.GetRequiredService<IUndoRedoManager>();
            var propertyValue = expression.ResolvedSource.GetType().GetProperty(expression.ResolvedSourcePropertyName).GetValue(viewModel);

            if (!undoRedoManager.CanRedo || value == expression.ResolvedSource)
                return;

            object convertedValue;

            try
            {
                var targetType = Nullable.GetUnderlyingType(dp.PropertyType) ?? dp.PropertyType;

                if (propertyValue == null || targetType.IsInstanceOfType(propertyValue))
                {
                    convertedValue = propertyValue;
                }
                else
                {
                    convertedValue = Convert.ChangeType(propertyValue, targetType);
                }
            }
            catch
            {
                convertedValue = propertyValue;
            }

            if (value == propertyValue)
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
