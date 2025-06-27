using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public abstract class ToneUtil : IDisposable
{
    public readonly int sampleRate;

    public readonly int bufferSubunitSize;

    [NativeDisableParallelForRestriction]
    protected NativeArray<float> mainBuffer;
    protected NativeArray<float> whiteBuffer;

    public volatile bool isSafe = true;
    protected readonly ChordGenerator MajorChord;
    protected readonly ChordGenerator MinorChord;

    public NativeArray<Unity.Mathematics.Random> randoms;

    protected ToneUtil(int bufferSubunitRatio, int NumberOfBufferSubunits, ChordGenerator major, ChordGenerator minor)
    {
        sampleRate = AudioSettings.outputSampleRate;

        AudioSettings.GetDSPBufferSize(out int dspBufferSize, out _);
        bufferSubunitSize = (int)((sampleRate * 0.1f + dspBufferSize - 1) / dspBufferSize) * dspBufferSize * bufferSubunitRatio;

        mainBuffer = new(bufferSubunitSize * NumberOfBufferSubunits, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        whiteBuffer = new NativeArray<float>(mainBuffer, Allocator.Persistent);

        MajorChord = major;
        MinorChord = minor;

        randoms = new NativeArray<Unity.Mathematics.Random>(bufferSubunitSize * NumberOfBufferSubunits, Allocator.Persistent);
        Unity.Mathematics.Random seeder = new(1);
        for (int i = 0; i < 7 * bufferSubunitSize; i++) randoms[i] = new Unity.Mathematics.Random(seeder.NextUInt() + 1);
    }

    public JobHandle RefreshWhiteBuffer()
    {
        return new GenerateRandomToneJob
        {
            maxAmplitude = 0.5f,
            randoms = randoms,
            samples = whiteBuffer
        }.Schedule(whiteBuffer.Length, 64);
    }

    public abstract void RefreshBuffer(float freq, bool isMajor, Vector3Int gapSize);

    public virtual void Dispose()
    {
        mainBuffer.Dispose();
        whiteBuffer.Dispose();
        randoms.Dispose();
    }

    public NativeArray<float> MainBuffer => mainBuffer;
    public NativeArray<float> WhiteBuffer => whiteBuffer;
}

public class DisjointNoteTermination : ToneUtil
{
    public DisjointNoteTermination(int bufferSubunitRatio, ChordGenerator major, ChordGenerator minor) :
        base(bufferSubunitRatio, 5, major, minor) { }

    public override void RefreshBuffer(float freq, bool isMajor, Vector3Int gapSize)
    {
        
        double[] notes = isMajor ? MajorChord(freq) : MinorChord(freq);

        int[] constituentBufferSizes = new int[]
        {
            bufferSubunitSize * (5 - gapSize.x),
            bufferSubunitSize * (5 - gapSize.y),
            bufferSubunitSize * (5 - gapSize.z)
        };

        NativeArray<float>[] constituentBuffers = new NativeArray<float>[3];
        JobHandle[] Jobs = new JobHandle[3];
        JobHandle previous = default;

        isSafe = false;
        for (int i = 0; i < mainBuffer.Length; i++) mainBuffer[i] = 0f;
        for (int i = 0; i < 3; i++)
        {
            constituentBuffers[i] = new(constituentBufferSizes[i], Allocator.TempJob, NativeArrayOptions.ClearMemory);
            JobHandle mergeJob = new MergeBufferJob
            {
                source = constituentBuffers[i],
                destination = mainBuffer
            }.Schedule(constituentBufferSizes[i], 64, JobHandle.CombineDependencies(
                new GenerateToneJob
                {
                    frequency = notes[i],
                    sampleRate = sampleRate,
                    samples = constituentBuffers[i]
                }.Schedule(constituentBufferSizes[i], 64),
                previous
            ));
            previous = mergeJob;
            Jobs[i] = constituentBuffers[i].Dispose(mergeJob);

        }
        foreach (JobHandle job in Jobs) job.Complete();
        isSafe = true;
    }
}

public class SequentialSingleTone : ToneUtil
{
    public SequentialSingleTone(int bufferSubunitRatio, ChordGenerator major, ChordGenerator minor) :
        base(bufferSubunitRatio, 7, major, minor) {
        randoms = new NativeArray<Unity.Mathematics.Random>(7 * bufferSubunitSize, Allocator.Persistent);
        Unity.Mathematics.Random seeder = new(1);
        for (int i = 0; i < 7 * bufferSubunitSize; i++) randoms[i] = new Unity.Mathematics.Random(seeder.NextUInt()+1);
    }

    public override void RefreshBuffer(float freq, bool isMajor, Vector3Int gapSize)
    {
        double[] notes = isMajor ? MajorChord(freq) : MinorChord(freq);

        int constituentBufferSize = bufferSubunitSize;

        int[] selection = new int[] { gapSize.x, gapSize.y, gapSize.z };
        JobHandle[] Jobs = new JobHandle[3];

        isSafe = false;
        JobHandle whiteHandle = RefreshWhiteBuffer();
        for (int i = 0; i < mainBuffer.Length; i++) mainBuffer[i] = 0f;
        

        for (int i = 0; i < 3; i++)
        {
            var constituentarray = mainBuffer.GetSubArray(i * constituentBufferSize, constituentBufferSize);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref constituentarray, AtomicSafetyHandle.Create());
            Jobs[i] = new GenerateToneJob
            {
                frequency = notes[selection[i]],
                sampleRate = sampleRate,
                samples = constituentarray,
            }.Schedule(constituentBufferSize, 64);
        }

        foreach (JobHandle job in Jobs) job.Complete();
        whiteHandle.Complete();
        isSafe = true;
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}