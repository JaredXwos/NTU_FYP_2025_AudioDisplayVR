using Unity.Mathematics;
using UnityEngine;

public class AngleDistanceChirpGeneratorInput : MonoBehaviour, IChirpGeneratorInput
{
    public IHasAngle AngleProvider;
    public IHasDistance DistanceProvider;

    [SerializeField] private float MaximumSourceDistance = 5f;
    [SerializeField, Range(100, 1500)] private float MinFreq = 800f;
    [SerializeField, Range(100, 5000)] private float MaxFreq = 2000f;
    [SerializeField] private int MilisecondsPerUnitDistance = 1000;
    [SerializeField] private float sampleRate;

    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IHasAngle>(this, "AngleProvider");
        Check.PropertyEnabledElseAssign<IHasDistance>(this, "DistanceProvider");
        sampleRate = AudioSettings.outputSampleRate;
    }

    public ChirpGeneratorInput ChirpInput
    {
        get
        {
            float angle = math.clamp(AngleProvider.Angle, -math.PI, math.PI);
            float distance = math.max(0f, DistanceProvider.Distance);
            if (!math.isfinite(angle) || !math.isfinite(distance))
                return default;
            float norm = math.abs(angle) / math.PI;  // 0 at 0, 1 at 

            // X runs linearly left right
            float x = math.lerp(0f, MaximumSourceDistance, angle / (math.PI / 2f));
            x = math.clamp(x, -MaximumSourceDistance, MaximumSourceDistance);

            // Y drops linearly to Max at pi, peaks at 0 in front
            float y = math.lerp(0f, -MaximumSourceDistance, norm);

            float2 source = new(x, y);
            float endFreq = math.lerp(MaxFreq, MinFreq, norm);

            // Duration from distance
            float ms = distance * MilisecondsPerUnitDistance;
            int durationFrames = (int)(math.min(ms, 1000) / 1000f * sampleRate);

            return new ChirpGeneratorInput(durationFrames, (MaxFreq+MinFreq)/2, endFreq, source);
        }
    }
}