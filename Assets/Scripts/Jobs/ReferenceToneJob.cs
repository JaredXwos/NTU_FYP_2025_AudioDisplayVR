using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct ReferenceToneJob : IJobParallelFor
{
    public float frequency;
    public float sampleRate;
    public double timeOffset;

    public NativeArray<float> samples;

    public void Execute(int i)
    {
        double t = (i + timeOffset) / sampleRate;
        samples[i] = Mathf.Sin((float)(2 * Mathf.PI * frequency * t));
    }
}
