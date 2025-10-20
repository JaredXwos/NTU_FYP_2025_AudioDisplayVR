using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class NativeArrayFloatQueue : NativeArrayQueue<float> { }
public enum ChirpType
{
    [InspectorName("Exponential Sweep")]
    Exponential,

    [InspectorName("Linear Sweep")]
    Linear
}

public class ChirpGenerator : MonoBehaviour, IHasNativeQueue<float>
{
    [SerializeField] private ChirpType chirpType = ChirpType.Exponential;
    [SerializeField] private float sampleRate;
    [SerializeField] private float speedOfSound = 343f;
    [SerializeField, Range(0f, 1f)] private float centerAttenuationFactor = 0.5f;
    [SerializeField, Range(100, 5000)] private int minimumChirpDuration;
    [SerializeField, Range(6000, 30000)] private float maximumSignalDuration;
    [SerializeField, Range(0, 4800000)] private int minimumQueuedFrames = 10000;
    [SerializeField, Range(1, 100)] private int updateCount;

    public NativeArrayQueue<float> NativeQueue { get; private set; }
    private IChirpGeneratorInput InputProvider;

    public int ChirpDurationFrames => (int) math.round((minimumChirpDuration / 1000f) * sampleRate);
    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IChirpGeneratorInput>(this, "InputProvider");
        sampleRate = AudioSettings.outputSampleRate;
        // Initialize the queue with a generator that will be filled later in Fill()
        NativeQueue = gameObject.AddComponent<NativeArrayFloatQueue>();
        NativeQueue.Initialize(Fill, threshold: minimumQueuedFrames,updateCount);
    }
    /// <summary>
    /// Generates a chirp of fixed length, pads remainder with silence (by default allocation),
    /// applies spatialisation, interleaves, and returns the interleaved buffer.
    /// </summary>
    public NativeArray<float> Fill()
    {
        ChirpGeneratorInput input = InputProvider.ChirpInput;
        if(input.Duration == 0) return new NativeArray<float>(0, Allocator.Persistent);

        if (input.Duration > maximumSignalDuration / 1000f * sampleRate)
        {
            Debug.LogWarning($"ChirpGenerator: Input duration {input.Duration} frames exceeds maximum signal duration {maximumSignalDuration}ms; clamping.");
            input = new ChirpGeneratorInput((int)(maximumSignalDuration / 1000f * sampleRate), input.StartFreq, input.EndFreq, input.Source);
        }
        if(input.StartFreq > sampleRate / 2f || input.EndFreq > sampleRate / 2f)
        {
            Debug.LogWarning($"ChirpGenerator: Input frequencies ({input.StartFreq}Hz to {input.EndFreq}Hz) exceed Nyquist frequency ({sampleRate/2f}Hz); clamping.");
            float startFreq = math.clamp(input.StartFreq, 0f, sampleRate / 2f);
            float endFreq = math.clamp(input.EndFreq, 0f, sampleRate / 2f);
            input = new ChirpGeneratorInput(input.Duration, startFreq, endFreq, input.Source);
        }

        int totalFrames = input.Duration;
        int chirpFrames = math.min(ChirpDurationFrames, totalFrames);

        // --- Step 1: Allocate full mono buffer (zeroed)
        var raw = new NativeArray<float>(totalFrames, Allocator.TempJob, NativeArrayOptions.ClearMemory);

        // --- Step 2: Generate chirp into front portion
        var genHandle = chirpType switch
        {
            ChirpType.Exponential => new GenerateExponentialChirp
            {
                Output = raw.GetSubArray(0, chirpFrames),
                StartFreq = input.StartFreq,
                EndFreq = input.EndFreq,
                SampleRate = sampleRate
            }.Init().Schedule(chirpFrames, 64),
            ChirpType.Linear => new GenerateLinearChirp
            {
                Output = raw.GetSubArray(0, chirpFrames),
                StartFreq = input.StartFreq,
                EndFreq = input.EndFreq,
                SampleRate = sampleRate
            }.Init().Schedule(chirpFrames, 64),
            _ => throw new ArgumentOutOfRangeException(),
        };

        // --- Step 3: Compute spatialisation params
        var spatialParams = new NativeReference<SpatializationParams>(Allocator.TempJob);
        JobHandle spHandle = new CreateSpatialisationParamsJob
        {
            rightEar = new float2(0.1f, 0f), // ~0.2m head width
            source = input.Source,
            speedOfSound = speedOfSound,
            sampleRate = sampleRate,
            centreAttenuationFactor = centerAttenuationFactor,
            Params = spatialParams
        }.Schedule();

        // --- Step 4: Spatialize into left/right buffers
        var left = new NativeArray<float>(totalFrames, Allocator.TempJob);
        var right = new NativeArray<float>(totalFrames, Allocator.TempJob);

        JobHandle spatialHandle = new SpatializeAddJob
        {
            input = raw,
            Params = spatialParams,
            outputLeft = left,
            outputRight = right
        }.Schedule(totalFrames, 64, JobHandle.CombineDependencies(genHandle, spHandle));

        // --- Step 5: Interleave into stereo buffer
        var interleaved = new NativeArray<float>(totalFrames * 2, Allocator.Persistent);
        var interleaveJob = new InterleaveStereoJob
        {
            Left = left,
            Right = right,
            Output = interleaved
        };
        JobHandle interleaveHandle = interleaveJob.Schedule(totalFrames, 64, spatialHandle);

        // --- Step 6: Complete jobs and clean up
        interleaveHandle.Complete();

        raw.Dispose();
        left.Dispose();
        right.Dispose();
        spatialParams.Dispose();

        return interleaved;
    }

    private void OnDestroy()
    {
        if (NativeQueue != null) Destroy(NativeQueue);
        NativeQueue = null;
    }
}