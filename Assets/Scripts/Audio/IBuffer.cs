using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class RingBuffer<T> : IDisposable where T : unmanaged
{

    protected NativeArray<T> array;
    public readonly int Length;
    public int StartPosition { get; private set; }
    public RingBuffer(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        Length = length;
        array = new NativeArray<T>(length, Allocator.Persistent);
        InterfaceRegistry<IDisposable>.Register(this);
    }

    public void Dispose()
    {
        if (array.IsCreated) array.Dispose();
        InterfaceRegistry<IDisposable>.Unregister(this);
    }

    public T this[int i]
    {
        get { return array[(StartPosition + i) % Length]; }
        set { array[(StartPosition + i) % Length] = value; }
    }

    public NativeSlice<T>[] Read(RingBuffer<T>[] matching = default)
    {
        SortedSet<int> boundaries = new() { StartPosition, Length };

        foreach (int boundary in 
            matching
            .SelectMany(rb => new[]{rb.Length - rb.StartPosition,rb.Length})
            .Where(b => b < Length)
            .Select(b => (b + StartPosition) % Length)
        ) boundaries.Add(boundary);

        List<NativeSlice<T>> result = new();
        int prev = StartPosition;
        foreach (int boundary in boundaries.Where(b => b > StartPosition))
        {
            result.Add(array.Slice(prev, boundary - prev));
            prev = boundary;
        }
        prev = 0;
        foreach (int boundary in boundaries.Where(b => b <= StartPosition))
        {
            result.Add(array.Slice(prev, boundary - prev));
            prev = boundary;
        }

        return result.ToArray();
    }

    public void Append(NativeSlice<T> data)
    {
        if (data.Length == 0) return;
        if (data.Length > Length) throw new ArgumentOutOfRangeException(nameof(data), "Data length exceeds buffer length.");

        if (data.Length + StartPosition <= Length)
            array.Slice(StartPosition, data.Length).CopyFrom(data);
        else
        {
            array.Slice(StartPosition, Length - StartPosition).CopyFrom(data.Slice(0, Length - StartPosition));
            array.Slice(0, data.Length - (Length - StartPosition)).CopyFrom(data.Slice(Length - StartPosition));
        }
        StartPosition = (StartPosition + data.Length) % Length;
    }

    public void Append(NativeSlice<T>[] data)
    {
        if(data.Select(s => s.Length).Sum() > Length) 
            throw new ArgumentOutOfRangeException(nameof(data), "Total data length exceeds buffer length.");
        foreach (NativeSlice<T> slice in data)
            Append(slice);
    }

    public void Write(NativeSlice<T> data)
    {
        if (data.Length == 0) return;
        if (data.Length > Length) throw new ArgumentOutOfRangeException(nameof(data), "Data length exceeds buffer length.");
        StartPosition = 0;
        array.Slice(0, data.Length).CopyFrom(data);
    }

    public void Write(NativeSlice<T>[] data)
    {
        if (data.Select(s => s.Length).Sum() > Length)
            throw new ArgumentOutOfRangeException(nameof(data), "Total data length exceeds buffer length.");
        Write(data[0]);
        for (int i = 1; i < data.Length; i++)
            Append(data[i]);
    }

    public int CopySliceTo(T[] destination, int read)
    {
        if (destination.Length > Length) throw new ArgumentOutOfRangeException(nameof(destination), "Destination length exceeds buffer length.");
        if (read + destination.Length > Length)
        {
            NativeArray<T>.Copy(array, read, destination, 0, Length - read);
            NativeArray<T>.Copy(array, 0, destination, Length - read, destination.Length - (Length - read));
        }
        else NativeArray<T>.Copy(array, read, destination, 0, destination.Length);
        return (read + destination.Length) % Length;
    }

    public void Clear() => array.AsSpan().Clear();
}

