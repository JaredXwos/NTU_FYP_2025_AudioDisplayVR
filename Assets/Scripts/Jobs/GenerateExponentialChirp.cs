using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GenerateExponentialChirp: IJobParallelFor
{
    public NativeArray<float> Output;

    public float StartFreq;
    public float EndFreq;
    public float SampleRate;

    // Precomputed fields
    private float _duration;
    private float _ratio;
    private float _invLnRatioOrLinear; // behaves like 1/log(ratio), or just 1 when ratio==1

    public GenerateExponentialChirp Init()
    {
        _duration = Output.Length / SampleRate;

        if (math.abs(EndFreq - StartFreq) < 1e-6f)
        {
            _ratio = 1f;
            _invLnRatioOrLinear = 1f; // treat as linear in t
        }
        else
        {
            _ratio = EndFreq / StartFreq;
            _invLnRatioOrLinear = 1f / math.log(_ratio);
        }

        return this;
    }

    public void Execute(int index)
    {
        int N = Output.Length;
        if (N <= 1 || StartFreq <= 0f || EndFreq <= 0f)
        {
            Output[index] = 0f;
            return;
        }

        float t = (float)index / (N - 1);

        // This works even if ratio == 1 (since pow(1,t) - 1 = 0, but invLnRatioOrLinear = 1)
        float cycles;
        if (_ratio == 1f)
        {
            // Pure tone formula
            float time = index / SampleRate;
            cycles = StartFreq * time;
        }
        else
        {
            // Exponential sweep
            cycles = StartFreq * _duration * (math.pow(_ratio, t) - 1f) * _invLnRatioOrLinear;
        }

        float phase = 2f * math.PI * cycles;
        Output[index] = math.sin(phase);
    }
}