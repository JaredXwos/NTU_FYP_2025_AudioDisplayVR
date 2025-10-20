public interface IHasNativeQueue<T> where T : unmanaged
{
    NativeArrayQueue<T> NativeQueue { get; }
}