using Unity.Collections;
using System;

public abstract class BufferProcessor<T> : IDisposable where T : unmanaged
{
    public static int samplerate = -1;
    protected NativeArray<T>[] input;
    protected NativeArray<T>[] output;
    protected BufferProcessor<T> next;
    private bool isInitialised = false;

    public BufferProcessor(NativeArray<T>[] input)
        => this.input = input ?? throw new ArgumentNullException(nameof(input), "Input cannot be null");
    public BufferProcessor(BufferProcessor<T> next)
    {
        if (next == null) throw new ArgumentNullException(nameof(next), "Other cannot be null");
        input = next.output;
        this.next = next;
    }


    public void Initialise()
    {
              
        if (isInitialised) return;

        if (samplerate < 0) 
            throw new InvalidOperationException("Samplerate must be set before initialising the buffer processor.");

        if (ArrayCount.inputArrayCount < 1)
            throw new ArgumentException("At least one input array is required.");

        if (input.Length < ArrayCount.inputArrayCount)
            throw new ArgumentException($"At least {ArrayCount.inputArrayCount} number of arrays required in input.");

        int length = -1;
        foreach (NativeArray<T> array in input)
            if (!array.IsCreated)
                throw new ArgumentException("Input buffer not initialised");
            else if (length == -1) length = array.Length;
            else if (length != array.Length)
                throw new ArgumentException("Not all input buffers are the same length");
        if (length < 0)
            throw new ArgumentOutOfRangeException("Buffer length must be more than zero");

        output = new NativeArray<T>[ArrayCount.outputArrayCount];
        for (int i = 0; i < output.Length; i++)
            output[i] = new NativeArray<T>(length, Allocator.Persistent);
        isInitialised = true;
    }

    public virtual void Dispose()
    {
        if (next != null) next.Dispose();
        if (output != null)
        {
            foreach (var array in output)
                if (array.IsCreated)
                    array.Dispose();
            output = null;
        }
        isInitialised = false;
    }

    public NativeArray<T>[] Output
    {
        get
        {
            if (next != null) return next.Output;
            if (output == null)
                throw new ObjectDisposedException("Output not initialised");
            return output;
        }
    }
    public NativeArray<T>[] Input
    {
        get
        {
            if (input == null)
                throw new ObjectDisposedException("Input not initialised");
            return input;
        }
    }

    public void Process()
    {
        if (!isInitialised)
            Initialise();
        InternalProcess();
        if (next != null)
            next.Process();
    }


    protected abstract (int inputArrayCount, int outputArrayCount) ArrayCount { get; }

    protected abstract void InternalProcess();
}