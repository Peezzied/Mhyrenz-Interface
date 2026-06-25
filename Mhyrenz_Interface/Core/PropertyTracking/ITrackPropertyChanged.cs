namespace Mhyrenz_Interface.Core.PropertyTracking
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
        public TrackedPropertyChangedEventArgs(object newValue, object oldValue, string propertyName, PropertyChangeOrigin origin)
        {
            NewValue = newValue;
            OldValue = oldValue;
            PropertyName = propertyName;
            Origin = origin;
        }

        public object NewValue { get; set; }
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
