using System;
using UnityEngine;

/// <summary>
/// Base class for binaural audio consumers. 
/// Expects the queue to provide interleaved stereo frames [L0, R0, L1, R1, ...].
/// If Unity provides anything other than 2 channels, silence is output instead.
/// </summary>
public class NativeQueueAudioBroadcaster : MonoBehaviour
{
    private IHasNativeQueue<float> Provider;

    private void Awake() => Check.PropertyEnabledElseAssign<IHasNativeQueue<float>>(this, "Provider");

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2)
        {
            // Fill with silence and warn
            Array.Clear(data, 0, data.Length);
            Debug.LogWarning($"[{nameof(NativeQueueAudioBroadcaster)}] Non-binaural channel count ({channels}) detected. Outputting silence.");
            return;
        }

        var queue = Provider?.NativeQueue;
        if (queue == null || data == null) return;

        // Unity wants `frames * channels` samples, and queue provides them already interleaved.
        int samplesNeeded = data.Length;
        var slices = queue.Get(samplesNeeded);

        int writePos = 0;
        foreach (var slice in slices)
            for (int i = 0; i < slice.Length; i++)
                if (writePos < data.Length)
                    data[writePos++] = slice[i];
    }
}