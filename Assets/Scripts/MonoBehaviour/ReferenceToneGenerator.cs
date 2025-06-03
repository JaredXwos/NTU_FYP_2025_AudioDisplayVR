using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

[RequireComponent(typeof(AudioSource))]
public class ReferenceToneGenerator : MonoBehaviour
{
    //  Constants / Configuration
    private const int samplesPerJob = 1024;         // Number of samples per job chunk
    private const int jobLeadCount = 10;            // Jobs ahead of reader (write-read gap)

    //  User-Controlled Parameters
    [Tooltip("Frequency of the generated sine wave in Hz")]
    public float frequency = 440f;                  // Sine wave frequency
    public double timeOffset = 0;                   // Time origin for waveform continuity

    //  Audio Context
    private float sampleRate = 0;                   // System sample rate (set at runtime)
    private NativeArray<float>[] jobBuffers;        // Shared audio buffer (written by jobs)

    //  Job Scheduling
    private List<JobHandle> jobHandles;   // Queue of active jobs

    //  Buffer Position Tracking
    private int readPointer = 0;                    // Read position for audio system
    private int writePointer = 0;                   // Write position for next job

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        timeOffset += AudioSettings.dspTime;
        jobBuffers = new NativeArray<float>[jobLeadCount];
        jobHandles = new List<JobHandle>();
        AudioSettings.GetDSPBufferSize(out int bufferSize, out int _);
        for (int i = 0; i < jobLeadCount; i++)
        {
            jobBuffers[i] = new NativeArray<float>(bufferSize, Allocator.Persistent);
            jobHandles.Add(ScheduleJob());
            jobHandles[i].Complete();
        }
    }

    JobHandle ScheduleJob()
    {
        ReferenceToneJob job = new()
        {
            frequency = frequency,
            sampleRate = sampleRate,
            timeOffset = timeOffset,
            samples = jobBuffers[writePointer++],
        };
        writePointer %= jobLeadCount;
        timeOffset += samplesPerJob;
        return job.Schedule(samplesPerJob, 64);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        jobHandles[readPointer].Complete();
        // For each frame
        for (int i = 0; i < data.Length; i += channels)
            // For each channel
            for (int c = 0; c < channels; c++)
                data[i + c] = jobBuffers[readPointer][i / channels];

        jobHandles[readPointer++] = ScheduleJob();
        readPointer %= jobLeadCount;
    }

    void OnDestroy()
    {
        foreach(JobHandle handle in jobHandles) handle.Complete();
        foreach(NativeArray<float> jobBuffer in  jobBuffers)
            if (jobBuffer.IsCreated)
                jobBuffer.Dispose();
    }
}
