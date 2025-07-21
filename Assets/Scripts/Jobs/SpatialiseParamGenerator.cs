using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[System.Serializable]
public struct SpatializationParams
{
    public int shiftL;
    public int shiftR;
    public float gainL;
    public float gainR;
    public float centerGain;

    public float distToLeft;
    public float distToRight;
    public float distanceFactor;
    public float delaySamplesR;
    public float delaySamplesL;

    public static SpatializationParams Create(
    float2 rightEar,
    float2 source,
    float speedOfSound,
    float sampleRate,
    float centerAttenuationFactor)
    {
        SpatializationParams p;

        // Calculate distances
        p.distToLeft = math.length(source + rightEar);
        p.distToRight = math.length(source - rightEar);

        float delaySecR = p.distToRight / speedOfSound;
        float delaySecL = p.distToLeft / speedOfSound;

        p.delaySamplesR = delaySecR * sampleRate;
        p.delaySamplesL = delaySecL * sampleRate;

        float minDelay = math.min(p.delaySamplesL, p.delaySamplesR);
        p.shiftR = (int)(p.delaySamplesR - minDelay);
        p.shiftL = (int)(p.delaySamplesL - minDelay);

        p.gainL = 1f / (p.distToLeft * p.distToLeft + 1e-5f);
        p.gainR = 1f / (p.distToRight * p.distToRight + 1e-5f);

        float dist = math.max(p.distToLeft, p.distToRight) - 0.5f * (p.distToLeft + p.distToRight);
        float headDiameter = math.length(rightEar);
        p.distanceFactor = math.clamp(dist / headDiameter, 0f, 1f);
        p.centerGain = math.lerp(1f, p.distanceFactor, centerAttenuationFactor);

        return p;
    }
}

public struct CreateSpatialisationParamsJob : IJob
{
    [ReadOnly] public float2 rightEar;
    [ReadOnly] public float2 source;
    [ReadOnly] public float speedOfSound;
    [ReadOnly] public float sampleRate;
    [ReadOnly] public float centreAttenuationFactor;

    public NativeReference<SpatializationParams> Params;
    public void Execute()
    {
        Params.Value = SpatializationParams.Create(rightEar, source, speedOfSound, sampleRate, centreAttenuationFactor);
    }
}