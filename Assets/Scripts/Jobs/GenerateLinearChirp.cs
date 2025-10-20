using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GenerateLinearChirp : IJobParallelFor
{
    public NativeArray<float> Output;

    public float StartFreq;
    public float EndFreq;
    public float SampleRate;

    public readonly GenerateLinearChirp Init() => this;

    public void Execute(int index)
    {
        int N = Output.Length;
        float duration = N / SampleRate;

        // Normalized time 0..1
        float t = (float)index / (N - 1);

        // Integral of frequency over time -> phase in cycles
        float cycles = StartFreq * t + 0.5f * (EndFreq - StartFreq) * t * t;

        // Phase in radians
        float phase = 2f * math.PI * duration * cycles;

        // Sample value (sine wave)
        Output[index] = math.sin(phase);
    }
}