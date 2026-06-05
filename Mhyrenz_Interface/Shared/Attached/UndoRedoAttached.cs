using System;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using Control = System.Windows.Controls.Control;
using DataGrid = System.Windows.Controls.DataGrid;
using DatePicker = System.Windows.Controls.DatePicker;
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

        private static DataGrid dataGrid;
        private static void OnUndoRedoBound(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!(bool)e.NewValue) return;
                var control = d as Control;

                dataGrid = TreeHelper.TryFindParent<DataGrid>(control);

                switch (d)
                {
                    case TextBox textBox:
                        TrackControl(textBox, TextBox.TextProperty, TextBox.UnloadedEvent);
                        break;
                    case NumericUpDown numericUpDown:
                        TrackControl(numericUpDown, NumericUpDown.ValueProperty, NumericUpDown.LostFocusEvent);
                        break;
                    case DatePicker datePicker:
                        TrackControl(datePicker, DatePicker.SelectedDateProperty, DatePicker.LostFocusEvent);
                        break;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private static void TrackControl(Control control, DependencyProperty dp, RoutedEvent routedEvent)
        {
            var cell = TreeHelper.TryFindParent<System.Windows.Controls.DataGridCell>(control);
            RoutedEventHandler unloadedHandler;
            RoutedEventHandler handler = (s, _) =>
            {

                //TryUpdateSource(control, dp);
            };

            control.AddHandler(routedEvent, handler);
            _controlEvents.Add(control, handler);

            unloadedHandler = (_, __) =>
            {
                if (_controlEvents.TryGetValue(control, out var h))
                {
                    control.AddHandler(routedEvent, h as RoutedEventHandler);
                    _controlEvents.Remove(control);
                }
            };

            control.AddHandler(routedEvent, unloadedHandler);

            var CleanupUnloaded = new RoutedEventHandler((s, _) =>
            {
                control.RemoveHandler(routedEvent, unloadedHandler);
            });

            control.AddHandler(routedEvent, CleanupUnloaded);
        }


    }
}
