using System;

[System.Serializable]
public class ObservableValue<T>
{
    public T _value;
    public event Action<T> OnValueChanged;

    public ObservableValue(T initialValue = default)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    // Optional: implicit conversion to simplify usage
    public static implicit operator T(ObservableValue<T> observable)
    {
        return observable._value;
    }
}