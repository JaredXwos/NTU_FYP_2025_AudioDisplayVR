using System.Threading;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class AudioGenerator : MonoBehaviour
{

    public bool IsPlaying;
    protected int framesPerCallback;

    [Header("Read Only display settings")]
    // ----------------------------------------------------------------------

    [Tooltip("The current read index of the output audio buffer")]
    [SerializeField] private int Read;

    [SerializeField] protected int sampleRate;

    [Tooltip("The total length of a single full buffer, subbuffer length * subbuffer count")]
    [SerializeField] protected int BufferTotalLength;
    protected int SubBufferLength;

    // Buffer structure configuration
    // ----------------------------------------------------------------------
    protected abstract int SubBufferCount { get; }

    [Tooltip("How long a sub buffer should be, in miliseconds")]
    protected abstract float SubBufferMinimumInterval { get; }

    // Buffers and Buffer indices
    // ----------------------------------------------------------------------
    protected RingBuffer<float> outputBuffers;
    protected volatile int read;
    protected volatile int write;

    // Thread Safety
    // ----------------------------------------------------------------------
    protected AutoResetEvent requestRefill = new(true);

    protected virtual void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        framesPerCallback = AudioSettings.GetConfiguration().dspBufferSize;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (IsPlaying)
            outputBuffers.CopySliceTo(data, read);
        if (
            read > write && read - write > BufferTotalLength / 2 ||
            write >= read && write - read <= BufferTotalLength / 2
        ) requestRefill.Set();
    }
    protected abstract void BufferRefresh();

    public static void CalculateBufferLengths(
        float sampleRate, 
        int SubBufferCount, 
        float SubBufferMinimumInterval, 
        int framesPerCallback, 
        
        out int SubBufferLength, 
        out int bufferTotalLength)
    {
        SubBufferLength = Mathf.CeilToInt(sampleRate * (SubBufferMinimumInterval / 1000f));
        SubBufferLength += framesPerCallback - (SubBufferLength % framesPerCallback);
        bufferTotalLength = SubBufferLength * SubBufferCount;
    }
}