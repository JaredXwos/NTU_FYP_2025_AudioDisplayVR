using NUnit.Framework.Constraints;
using System;
using System.Linq.Expressions;
using System.Numerics;

public interface IInPlaceBufferTransform<T> where T : unmanaged
{
    void Process(RingBuffer<T> buffer);
    void Process(RingBuffer<T>[] array);
}

public sealed class WriteFreqGradient : IInPlaceBufferTransform<float>
{
    private double phase = 0f;
    private readonly int sampleRate;
    public int SampleRate { private get; set; }
    public float FrequencyStart { private get; set; }
    public float FrequencyEnd { private get; set; }

    public void Process(RingBuffer<float> array)
    {
        int n = array.Length;
        if (n < 2) return;

        double f0 = FrequencyStart;
        double dfPerSample = (FrequencyEnd - FrequencyStart) / n;
        double twoPiOverFs = 2.0 * Math.PI / sampleRate;

        for (int i = 0; i < n; i++)
        {
            double f = f0 + dfPerSample * i; // instantaneous frequency
            phase += twoPiOverFs * f;        // integrate frequency -> phase
            // keep phase bounded to avoid precision loss on long runs
            if (phase > 1e6) phase = Math.IEEERemainder(phase, 2.0 * Math.PI);

            array[i] = (float)Math.Sin(phase); // overwrite; no gain here
        }
    }
    public void Process(RingBuffer<float>[] array)
    {
        foreach (RingBuffer<float> buffer in array)
            Process(buffer);
    }
}

public sealed class SpaceOut<T> : IInPlaceBufferTransform<T> where T : unmanaged
{
    public int Distance { private get; set; }
    public int Displacement { private get; set; }
    public void Process(RingBuffer<T> buffer) 
    {
        for (int i = buffer.Length - 1; i >= 0; i--)
            buffer[i] = i >= Distance && (i - Displacement) % Distance == 0? buffer[(i - Displacement) % Distance] : default;
    }
    public void Process(RingBuffer<T>[] array)
    {
        foreach(RingBuffer<T> buffer in array) Process(buffer);
    }
}

public sealed class Accumulate<T> : IInPlaceBufferTransform<T> where T : unmanaged
{
    public static Func<T, T, T> Add;

    Accumulate()
    {
        try
        {
            var a = Expression.Parameter(typeof(T));
            var b = Expression.Parameter(typeof(T));
            var body = Expression.Add(a, b);
            Add = Expression.Lambda<Func<T, T, T>>(body, a, b).Compile();
        }
        catch { throw new NotSupportedException($"{typeof(T)} does not support +"); }
    }
    public void Process(RingBuffer<T> buffer) { }
    public void Process(RingBuffer<T>[] array)
    {
        if (array == null || array.Length < 2) return;
        for (int i = 1; i < array.Length; i++)
            for (int j = 0; j < Math.Min(array[0].Length, array[i].Length); j++)
                array[0][j] = Add(array[0][j], array[i][j]);
    }
}