using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ObstacleSignalInputProvider : MonoBehaviour, IChirpGeneratorInput
{
    [SerializeField] private float MaximumSourceDistance = 5f;
    [SerializeField, Range(100, 1500)] private float MinFreq = 800f;
    [SerializeField, Range(100, 5000)] private float MaxFreq = 2000f;
    [SerializeField] private int MilisecondsPerUnitDistance = 1000;
    [SerializeField] private float sampleRate;
    [SerializeField] private float updatePrecision;

    private readonly IHasObstacleSignal SignalProvider;

    private ObstacleSignal currentSignal = ObstacleSignal.Empty;
    [SerializeField] private int currentDirectionIndex = 0;
    [SerializeField] private int currentDirectionCount = 0;
    [SerializeField] private float lastReportedDirection = 0;
    [SerializeField] private float lastReportedDistance = 0;

    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IHasObstacleSignal>(this, "SignalProvider");
        sampleRate = AudioSettings.outputSampleRate;
    }

    public ChirpGeneratorInput NextChirpInput()
    {
        UpdateCurrentDirections(
            currentSignal, currentDirectionIndex, 
            SignalProvider.ObstacleSignal,
            updatePrecision, 
            out currentSignal, out currentDirectionIndex);
        currentDirectionCount = currentSignal.directions.Length;
        if (currentSignal.directions.Length == 0)
            return new ChirpGeneratorInput(0, 685.3301f, 685.3301f, new float2(-0.2f,-0.99f));
        lastReportedDirection = currentSignal.directions[currentDirectionIndex];
        lastReportedDistance = currentSignal.distance;
        return CreateChirp(
            currentSignal.directions[currentDirectionIndex++],
            currentSignal.distance
            );
    }

    private ChirpGeneratorInput CreateChirp(float angle, float distance)
    {
        float norm = math.abs(angle) / math.PI;

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

        return new ChirpGeneratorInput(durationFrames, (MaxFreq + MinFreq) / 2, endFreq, source);
    }

    private static void UpdateCurrentDirections(
        ObstacleSignal oldSignal, int oldIndex, 
        ObstacleSignal newSignal, 
        float preci,
        out ObstacleSignal signal, out int index)
    {
        if (oldIndex >= oldSignal.directions.Length)
        {
            signal = newSignal;
            index = 0;
            return;
        }
        
        ObstacleSignal amendedSignal = new ObstacleSignal(
            Mathf.Min(newSignal.distance, oldSignal.distance),
            newSignal.directions.Concat(oldSignal.directions).ToArray()
            ).
            ReduceToCount(4, preci);
        index = FindNewIndex(
            amendedSignal.directions,
            oldSignal.directions[oldIndex],
            preci
            );
        signal = amendedSignal;
    }

    public static int FindNewIndex(float[] span, float val, float preci)
    {
        int idx = Array.BinarySearch(span, val);
        if (idx >= 0) return idx;

        idx = ~idx;

        // Insert Index 0, asd.Length are equivalent, currentValue closer to pi/-pi than all values in asd)
        if (idx == span.Length)
        {
            idx = 0;
            val -= 2 * Mathf.PI; //Move them to the front to treat as index 0 case
        }

        if (idx == 0) return // (asd[^1] - 2PI) < currentValue < asd[0] <...< asd[^1] < 2PI
            span[0] - val < val - (span[^1] - 2 * Mathf.PI) &&
            span[0] - val < preci ?
            0 : span.Length - 1;

        else return // asd[insertIndex - 1] < currentValue < asd[insertIndex] <...< asd[^1]
            span[idx] - val < val - span[idx - 1] &&
            span[idx] - val < preci ?
            idx : idx - 1;
    }

}