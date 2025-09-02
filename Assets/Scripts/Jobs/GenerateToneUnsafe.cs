using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct GenerateToneUnsafeJob : IJobParallelFor
{
    [ReadOnly] public double frequency;
    [ReadOnly] public float sampleRate;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<float> samples;

    public void Execute(int i)
    {
        samples[i] = Mathf.Sin((float)(2f * Mathf.PI * frequency * i / sampleRate));
    }
}
