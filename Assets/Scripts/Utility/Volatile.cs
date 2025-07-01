using System;

public class Volatile<T>
{
    private readonly object sync = new();
    private T _value;

    public Volatile(T value) => _value = value;
    public Volatile()
    {
        if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
            _value = (T)Activator.CreateInstance(typeof(T));
        else
            _value = default;
    }
    
    public T Value
    {
        get { lock (sync) return _value; }
        set { lock (sync) _value = value; }
    }
}