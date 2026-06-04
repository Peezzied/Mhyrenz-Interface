using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public delegate void TrackedPropertyChangedHandler(object sender, TrackedPropertyChangedEventArgs e);

    public enum PropertyChangeOrigin
    {
        User,
        UndoRedo,
        Programmatic
    }

    public class TrackedPropertyChangedEventArgs
    {
        public TrackedPropertyChangedEventArgs(object oldValue, string propertyName, PropertyChangeOrigin origin)
        {
            OldValue = oldValue;
            PropertyName = propertyName;
            Origin = origin;
        }

        public object OldValue { get; set; }
        public string PropertyName { get; set; }
        public PropertyChangeOrigin Origin { get; set; }

        public bool IsTrueOrigin => Origin != PropertyChangeOrigin.UndoRedo;
    }

    public interface ITrackPropertyChanged
    {
        event TrackedPropertyChangedHandler TrackedPropertyChanged;
    }
}
