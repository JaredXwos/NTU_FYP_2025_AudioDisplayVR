using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct GenerateToneJob : IJobParallelFor
{
    [ReadOnly] public double frequency;
    [ReadOnly] public float sampleRate;

    [NativeDisableParallelForRestriction]
    public NativeArray<float> samples;

    public void Execute(int i)
    {
        samples[i] = Mathf.Sin((float)(2f * Mathf.PI * frequency * i / sampleRate));
    }
}
