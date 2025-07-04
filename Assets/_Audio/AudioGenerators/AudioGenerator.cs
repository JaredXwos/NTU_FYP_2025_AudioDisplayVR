using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class AudioGenerator : MonoBehaviour
{
    [Header("Read Only display settings")]
    // ----------------------------------------------------------------------

    [Tooltip("The current read index of the output audio buffer")]
    [SerializeField, ReadOnly] private int Read;

    [SerializeField, ReadOnly] protected int sampleRate;

    [Tooltip("The total length of a single full buffer, subbuffer length * subbuffer count")]
    [SerializeField, ReadOnly] private int bufferTotalLength;

    // Buffer structure configuration
    // ----------------------------------------------------------------------
    protected abstract int ChannelCount { get; }

    protected abstract int SubBufferCount { get; }

    [Tooltip("How long a sub buffer should be, in miliseconds")]
    protected abstract float SubBufferMinimumInterval { get; }

    [Tooltip("Calculated smallest valid buffer length aligned with AudioFilterRead and longer than the minimum interval.")]
    protected int SubBufferLength { get; private set; }

    // Buffers and Buffer indices
    // ----------------------------------------------------------------------
    protected NativeArray<float>[][] outputBuffers = new NativeArray<float>[3][];
    protected volatile int readBufferIndex = 0;
    protected volatile int lastWritenBufferIndex = 0;
    private int read;

    // Cancellation token to ensure safe exit of buffer refresh
    // ----------------------------------------------------------------------
    private CancellationTokenSource tokenSource;  // This is to send the suicide instruction
    protected CancellationToken token;            // This is to receive the suicide instruction

    #region MonoBehavior
    protected virtual void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        int framesPerCallback = AudioSettings.GetConfiguration().dspBufferSize;

        SubBufferLength = Mathf.CeilToInt(sampleRate * (SubBufferMinimumInterval / 1000f));
        SubBufferLength += framesPerCallback - (SubBufferLength % framesPerCallback);

        bufferTotalLength = SubBufferLength * SubBufferCount;

        for (int i = 0; i < outputBuffers.Length; i++)
        {
            outputBuffers[i] = new NativeArray<float>[ChannelCount];
            for (int j = 0; j < ChannelCount; j++)
                outputBuffers[i][j] = new NativeArray<float>(bufferTotalLength, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }
        tokenSource = new();
        token = tokenSource.Token;
        Task.Run(BackgroundBufferRefresh);
    }

    protected virtual void Update() => Read = read;

    protected virtual void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length / channels; i++)
        {
            for (int c = 0; c < channels; c++)
                data[i * channels + c] = outputBuffers[readBufferIndex][c % ChannelCount][read];
            if (++read >= SubBufferCount * SubBufferLength)
            {
                read = 0;
                readBufferIndex = lastWritenBufferIndex;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        tokenSource.Cancel();
        foreach (NativeArray<float>[] output in outputBuffers)
            foreach (NativeArray<float> buffer in output)
                buffer.Dispose();
    }
    #endregion

    protected abstract void BackgroundBufferRefresh();
}
