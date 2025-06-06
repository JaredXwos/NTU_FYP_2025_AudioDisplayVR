using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ToneUtil
{
    public int sampleRate;
    public int bufferSubunitSize = 0;
    private NativeArray<float> mainBuffer;
    public volatile bool isSafe = true;
    private Func<float, Vector3> MajorChord;
    private Func<float, Vector3> MinorChord;


    public ToneUtil(int bufferSubunitRatio, Func<float, Vector3> major, Func<float, Vector3> minor)
    {
        sampleRate = AudioSettings.outputSampleRate;

        AudioSettings.GetDSPBufferSize(out int dspBufferSize, out _);
        bufferSubunitSize = (int) ((sampleRate * 0.1f + dspBufferSize - 1) / dspBufferSize) * dspBufferSize * bufferSubunitRatio;

        mainBuffer = new(bufferSubunitSize * 5, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        MajorChord = major;
        MinorChord = minor;
    }

    public void RefreshBuffer(float freq, bool isMajor, Vector3Int silenceDuration)
    {
        for (int i = 0; i < mainBuffer.Length; i++) mainBuffer[i] = 0f;
        Vector3 chord = isMajor? MajorChord(freq): MinorChord(freq);

        int[] constituentBufferSizes = new int[]
        {
            bufferSubunitSize * (5 - silenceDuration.x),
            bufferSubunitSize * (5 - silenceDuration.y),
            bufferSubunitSize * (5 - silenceDuration.z)
        };

        float[] notes = new float[] { chord.x, chord.y, chord.z };
        NativeArray<float>[] constituentBuffers = new NativeArray<float>[3];
        JobHandle[] Jobs = new JobHandle[3];
        JobHandle previous = default;
        
        isSafe = false;
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

    public NativeArray<float> MainBuffer => mainBuffer;
}
