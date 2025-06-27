using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GenerateRandomToneJob : IJobParallelFor
{
    [ReadOnly] public float maxAmplitude;
    public NativeArray<Random> randoms;
    public NativeArray<float> samples;


    public void Execute(int i)
    {
        Random rng = randoms[i];
        float noise = rng.NextFloat(-1f, 1f);
        randoms[i] = rng;
        samples[i] = noise * maxAmplitude;
    }
}