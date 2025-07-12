using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Controls.Attached
{
    public static class ValidationHelper
    {
        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.RegisterAttached(
            "HasError", typeof(bool), typeof(ValidationHelper), new PropertyMetadata(false));
        public static bool GetHasError(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasErrorProperty);
        }
        public static void SetHasError(DependencyObject obj, bool value)
        {
            obj.SetValue(HasErrorProperty, value);
        }

        static ValidationHelper()
        {
            EventManager.RegisterClassHandler(typeof(FrameworkElement), Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnValidationError));
        }

        private static void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                SetHasError(obj, Validation.GetHasError(obj));
            }
        }
    }
    public static class ParentValidationHelper
    {
        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.RegisterAttached(
            "HasError", typeof(bool), typeof(ParentValidationHelper), new PropertyMetadata(false));
        public static bool GetHasError(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasErrorProperty);
        }
        public static void SetHasError(DependencyObject obj, bool value)
        {
            obj.SetValue(HasErrorProperty, value);
        }
    }
}
