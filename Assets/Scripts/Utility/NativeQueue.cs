using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class NativeArrayQueue<T> : MonoBehaviour where T : unmanaged
{
    private struct ArrayEntry
    {
        public readonly NativeArray<T> Array; // owning array
        public int Offset;                    // how much has been consumed

        public ArrayEntry(NativeArray<T> array)
        {
            Array = array;
            Offset = 0;
        }

        public int Remaining => Array.Length - Offset;
        public bool IsFullyConsumed => Offset >= Array.Length;

        /// <summary>
        /// Takes up to <paramref name="requested"/> elements from this entry,
        /// advancing the offset and producing a slice.
        /// </summary>
        /// <param name="requested">How many elements we want to take.</param>
        /// <param name="slice">The resulting slice.</param>
        /// <returns>The actual number of elements taken.</returns>
        public int TakeSlice(int requested, out NativeSlice<T> slice)
        {
            int take = Math.Min(requested, Remaining);
            slice = new NativeSlice<T>(Array, Offset, take);
            Offset += take;
            return take;
        }
    }
    // [ Queues for active arrays and disposal lifecycle ]
    // ---------------------------
    // Active arrays, each with an offset tracking how much has been consumed.
    private readonly List<ArrayEntry> _arrays = new();
    // Fully-consumed arrays, staged for disposal (promoted next request cycle).
    private readonly List<NativeArray<T>> _disposeStaging = new();
    // Arrays ready to be freed; actually disposed in Update().
    private readonly List<NativeArray<T>> _disposeReady = new();

    // [ Configuration ]
    // ---------------------------
    // Function to generate new arrays when buffer is low.
    private Func<NativeArray<T>> _generateFunc;
    // Minimum number of valid elements to maintain across arrays.
    private int _threshold;
    private int _updateCount;
    // [ Counters / state tracking ]
    // ---------------------------
    // Total number of valid (unconsumed) elements across all active arrays.
    private int _validCount;
    // Number of arrays scheduled to be generated on the next Update().
    private bool _pendingGenerate => _validCount < _threshold;

    // CONSTRUCTOR
    public void Initialize(Func<NativeArray<T>> generateFunc, int threshold, int updateCount)
    {
        _generateFunc = generateFunc ?? throw new ArgumentNullException(nameof(generateFunc));
        _threshold = threshold;
        _validCount = 0;
        _updateCount = updateCount;
    }

    // PUBLIC METHODS AND PROPERTIES
    public int Count => _validCount;

    // Get a specified number of elements as NativeSlices.
    public NativeSlice<T>[] Get(int count)
    {
        ValidateRequest(count);
        PromoteStagedDisposals();

        List<NativeSlice<T>> slices = CollectSlices(count);

        return slices.ToArray();
    }

    // UNITY LIFECYCLE
    private void Update()
    {
        // Generate new arrays if flagged
        for (int i = 0; i < _updateCount; i++)
            if (_pendingGenerate)
            {
                NativeArray<T> arr = _generateFunc();
                if (arr != null && arr.IsCreated && arr.Length > 0)
                {
                    _arrays.Add(new ArrayEntry(arr));
                    _validCount += arr.Length;
                }
            }
            else break;

        // Dispose arrays marked for disposal
        for (int i = 0; i < _disposeReady.Count; i++)
        {
            var arr = _disposeReady[i];
            if (arr.IsCreated) arr.Dispose();
        }
        _disposeReady.Clear();
    }

    private void OnDestroy()
    {
        foreach (var entry in _arrays)
            if (entry.Array.IsCreated) entry.Array.Dispose();

        foreach (var arr in _disposeStaging)
            if (arr.IsCreated) arr.Dispose();

        foreach (var arr in _disposeReady)
            if (arr.IsCreated) arr.Dispose();

        _arrays.Clear();
        _disposeStaging.Clear();
        _disposeReady.Clear();
    }

    // PRIVATE HELPER METHODS
    private void ValidateRequest(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count),
                "Requested count must be > 0 and <= available valid count.");
    }

    private List<NativeSlice<T>> CollectSlices(int count)
    {
        List<NativeSlice<T>> slices = new();
        int remaining = count;

        while (_arrays.Count > 0 && remaining > 0)
        {
            ArrayEntry entry = _arrays[0];
            int taken = entry.TakeSlice(remaining, out var slice);
            slices.Add(slice);
            remaining -= taken;

            if (entry.IsFullyConsumed)
            {
                StageArrayForDisposal(entry.Array);
                _arrays.RemoveAt(0);
            }
            else _arrays[0] = entry; // write the updated offset back
        }
        _validCount -= count;

        return slices;
    }

    private void StageArrayForDisposal(NativeArray<T> array)
        => _disposeStaging.Add(array);
    

    private void PromoteStagedDisposals()
    {
        if (_disposeStaging.Count == 0) return;

        _disposeReady.AddRange(_disposeStaging);
        _disposeStaging.Clear();
    }
}