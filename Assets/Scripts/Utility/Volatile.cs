public class Volatile<T>
{
    private readonly object sync = new();
    private T _value;

    public Volatile(T value) => _value = value;
    public Volatile() => _value = default;
    public T Value
    {
        get { lock (sync) return _value; }
        set { lock (sync) _value = value; }
    }
}