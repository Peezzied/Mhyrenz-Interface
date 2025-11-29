using Mhyrenz_Interface;

public class PrimativeNotifyProperty<T> : BaseViewModel
{
    private T _value;

    public PrimativeNotifyProperty(T value = default)
    {
        _value = value;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    // Implicit conversion to T
    public static implicit operator T(PrimativeNotifyProperty<T> myValue)
    {
        return myValue._value;
    }

    // Implicit conversion from T
    public static implicit operator PrimativeNotifyProperty<T>(T value)
    {
        return new PrimativeNotifyProperty<T>(value);
    }
}