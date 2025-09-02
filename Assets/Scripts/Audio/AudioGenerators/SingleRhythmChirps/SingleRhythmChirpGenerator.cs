using UnityEngine;

public interface IRhythmInput
{
    public int SoundDuration { get; }
    public int SilenceDuration { get; }
}

public interface IBinauralFrequencyInput
{
    public float LeftFrequency { get; }
    public float RightFrequency { get; }
}
public class  SingleRhythmChirpGenerator : AudioGenerator
{
    [SerializeField] float StartFrequency = 660f;
    IRhythmInput rhythmInput;
    IBinauralFrequencyInput EndFrequencyInput;

    WriteFreqGradient writeFreqGradient;
    SpaceOut<float> spaceOut;

    protected override int SubBufferCount => 1;
    protected override float SubBufferMinimumInterval => 5000;

    private int SoundDuration;
    private RingBuffer<float> leftsignal;
    private RingBuffer<float> rightsignal;

    protected override void Awake()
    {
        base.Awake();
        CalculateBufferLengths(
            sampleRate, 
            SubBufferCount, 
            SubBufferMinimumInterval, 
            framesPerCallback, 
            
            out SubBufferLength, 
            out BufferTotalLength);
        Check.PropertyEnabledElseAssign<IRhythmInput>(this, "rhythmInput");
        Check.PropertyEnabledElseAssign<IBinauralFrequencyInput>(this, "EndFrequencyInput");
        writeFreqGradient = new WriteFreqGradient
        {
            SampleRate = sampleRate
        };
        SoundDuration = rhythmInput.SoundDuration;
        leftsignal = new RingBuffer<float>(SoundDuration);
        rightsignal = new RingBuffer<float>(SoundDuration);
    }

    protected override void BufferRefresh()
    {
        while (true)
        {
            requestRefill.WaitOne();
            writeFreqGradient.FrequencyStart = StartFrequency;
            writeFreqGradient.FrequencyEnd = EndFrequencyInput.LeftFrequency * 2;
            writeFreqGradient.Process(leftsignal);
            writeFreqGradient.FrequencyEnd = EndFrequencyInput.RightFrequency * 2;
            writeFreqGradient.Process(rightsignal);


        }
    }
}
