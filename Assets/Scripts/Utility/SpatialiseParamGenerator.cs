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

    public SpatializationParams(
    float2 rightEar,
    float2 source,
    float speedOfSound,
    float sampleRate,
    float centerAttenuationFactor)
    {

        // Calculate actual distances
        distToLeft = math.length(source + rightEar);
        distToRight = math.length(source - rightEar);

        // Convert distance difference to delay
        float delaySecR = distToRight / speedOfSound;
        float delaySecL = distToLeft / speedOfSound;

        delaySamplesR = delaySecR * sampleRate;
        delaySamplesL = delaySecL * sampleRate;

        float minDelay = math.min(delaySamplesL, delaySamplesR);
        shiftR = (int)(delaySamplesR - minDelay);
        shiftL = (int)(delaySamplesL - minDelay);

        gainL = 1f / (distToLeft * distToLeft + 1e-5f);
        gainR = 1f / (distToRight * distToRight + 1e-5f);

        // Center attenuation
        float dist = math.max(distToLeft, distToRight) - 0.5f * (distToLeft + distToRight);
        float headDiameter = math.length(rightEar);
        distanceFactor = math.clamp(dist / headDiameter, 0f, 1f);
        centerGain = math.lerp(1f, distanceFactor, centerAttenuationFactor);
    }
}