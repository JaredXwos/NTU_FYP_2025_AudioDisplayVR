using UnityEngine;

/// <summary>
/// Generates monaural beeps (one channel only).
/// Each beep sweeps from start frequency to end frequency.
/// Public controls:
/// - beepLengthSec: duration of each beep in seconds
/// - beepsPerMinute: cadence in beeps per minute (1–1200)
/// - startHz: frequency at beep start (200–1500 Hz)
/// - endHz: frequency at beep end (200–1500 Hz)
/// - panSide: Left or Right channel
/// - levelPercent: amplitude 0–100
/// - edgeFadeSec: fade at edges to avoid clicks
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioTest : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("Beep Pattern")]
    [Tooltip("Duration of each beep, in seconds.")]
    [Min(0.005f)] public float beepLengthSec = 0.15f;

    [Tooltip("Beep cadence, in beeps per minute (1–1200).")]
    [Range(1f, 1200f)] public float beepsPerMinute = 60f;

    [Header("Beep Sweep (Hz)")]
    [Tooltip("Start frequency of the beep (200–1500 Hz).")]
    [Range(200f, 1500f)] public float startHz = 400f;

    [Tooltip("End frequency of the beep (200–1500 Hz).")]
    [Range(200f, 1500f)] public float endHz = 800f;

    [Header("Output")]
    [Tooltip("Which side to play on (monaural).")]
    public Side panSide = Side.Left;

    [Tooltip("Output level (0–100).")]
    [Range(0, 100)] public int levelPercent = 80;

    [Header("Smoothing")]
    [Tooltip("Fade time at start/end of each beep (seconds) to avoid clicks.")]
    [Range(0.0f, 0.02f)] public float edgeFadeSec = 0.005f;

    private double _sampleRate;
    private double _phase;
    private double _timeInCycleSec;
    private double _cyclePeriodSec;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        RecomputeDerived();
    }

    private void OnValidate()
    {
        if (_sampleRate <= 0) _sampleRate = AudioSettings.outputSampleRate;
        if (beepLengthSec < 0.005f) beepLengthSec = 0.005f;

        // Clamp explicitly for safety
        beepsPerMinute = Mathf.Clamp(beepsPerMinute, 1f, 1200f);
        startHz = Mathf.Clamp(startHz, 200f, 1500f);
        endHz = Mathf.Clamp(endHz, 200f, 1500f);

        RecomputeDerived();
    }

    private void RecomputeDerived()
    {
        _cyclePeriodSec = 60.0 / Mathf.Max(1f, beepsPerMinute);
        if (beepLengthSec > _cyclePeriodSec)
            beepLengthSec = (float)_cyclePeriodSec;
        else beepLengthSec = Mathf.Min(0.15f, (float)_cyclePeriodSec);

        if (edgeFadeSec * 2f > beepLengthSec)
            edgeFadeSec = Mathf.Max(0f, (beepLengthSec * 0.49f));
    }

    private void Update()
    {
        RecomputeDerived();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels == 0) return;

        float level = Mathf.Clamp01(levelPercent / 100f);

        for (int i = 0; i < data.Length; i += channels)
        {
            bool beepOn = _timeInCycleSec < beepLengthSec;

            float env = 0f;
            if (beepOn)
            {
                if (edgeFadeSec > 0f)
                {
                    if (_timeInCycleSec < edgeFadeSec)
                        env = (float)(_timeInCycleSec / edgeFadeSec);
                    else if (_timeInCycleSec > (beepLengthSec - edgeFadeSec))
                        env = Mathf.Clamp01((float)((beepLengthSec - _timeInCycleSec) / edgeFadeSec));
                    else
                        env = 1f;
                }
                else env = 1f;
            }

            float sample = 0f;
            if (beepOn)
            {
                float t = (float)(_timeInCycleSec / beepLengthSec);

                // Linear sweep
                float freq = Mathf.Lerp(startHz, endHz, t);

                double phaseInc = 2.0 * Mathf.PI * freq / _sampleRate;

                sample = Mathf.Sin((float)_phase) * env * level;
                _phase += phaseInc;
                if (_phase > Mathf.PI * 2.0) _phase -= Mathf.PI * 2.0;
            }

            if (panSide == Side.Left)
            {
                data[i] = sample;
                if (channels > 1) data[i + 1] = 0f;
            }
            else
            {
                data[i] = 0f;
                if (channels > 1) data[i + 1] = sample;
            }

            _timeInCycleSec += 1.0 / _sampleRate;
            if (_timeInCycleSec >= _cyclePeriodSec)
            {
                _timeInCycleSec -= _cyclePeriodSec;
                _phase = 0.0;
            }
        }
    }
}