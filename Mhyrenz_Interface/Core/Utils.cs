using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mhyrenz_Interface.Core
{
    public static class Utils
    {
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T match)
                    return match;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static T FindParent<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T found)
                    return found;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
