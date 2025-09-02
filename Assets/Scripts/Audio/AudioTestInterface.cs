using UnityEngine;
public class AudioTestInterface : MonoBehaviour
{
    [Header("Driver Inputs")]
    [Tooltip("Distance controls BPM inversely: BPM = bpmAtDistance1 / Distance.")]
    [Range(1f, 1200f)] public float Distance = 1.0f;

    [Tooltip("Angle measured from the top (0° = up, 90° = right, 180° = down, 270° = left).")]
    [Range(0f, 359f)] public float Angle = 0.0f;

    [Header("Targets")]
    public AudioTest LeftAudioTest;
    public AudioTest RightAudioTest;

    [Header("Mapping Settings")]
    [Tooltip("BPM that should be produced when Distance == 1.")]
    [Range(1200f, 120000f)] public float bpmAtDistance1 = 60f;

    const float kMinDistance = 1e-3f;

    void Awake()
    {
        Check.PropertyEnabledElseAssign<AudioTest>(this, nameof(LeftAudioTest));
        Check.PropertyEnabledElseAssign<AudioTest>(this, nameof(RightAudioTest));
    }

    void Update()
    {
        if (LeftAudioTest == null || RightAudioTest == null) return;

        float bpm = bpmAtDistance1 / Mathf.Max(Distance, kMinDistance);
        bpm = Mathf.Clamp(bpm, 1f, 1200f);

        LeftAudioTest.beepsPerMinute = bpm;
        RightAudioTest.beepsPerMinute = bpm;

        float angleDeg = NormalizeDeg(Angle);
        float hz = EvaluateEndHz(angleDeg); 
        hz = Mathf.Clamp(hz, 200f, 1500f);

        LeftAudioTest.endHz = hz;
        RightAudioTest.endHz = hz;

        float rad = angleDeg * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);

        int leftLevel = Mathf.Clamp(Mathf.RoundToInt(50f * (1f - s)), 0, 100);
        int rightLevel = Mathf.Clamp(Mathf.RoundToInt(50f * (1f + s)), 0, 100);

        LeftAudioTest.levelPercent = leftLevel;
        RightAudioTest.levelPercent = rightLevel;
    }

    // --- Helpers ---

    static float NormalizeDeg(float deg)
    {
        // Wrap into [0,360)
        deg %= 360f;
        if (deg < 0f) deg += 360f;
        return deg;
    }

    static float EvaluateEndHz(float deg)
    {
        // Anchors every 90 degrees:
        // (0,1500) -> (90,650) -> (180,200) -> (270,650) -> (360,1500)
        // Linear interpolate within the current 90 segment.
        if (deg < 90f) return Mathf.Lerp(1500f, 650f, deg / 90f);
        else if (deg < 180f) return Mathf.Lerp(650f, 200f, (deg - 90f) / 90f);
        else if (deg < 270f) return Mathf.Lerp(200f, 650f, (deg - 180f) / 90f);
        else return Mathf.Lerp(650f, 1500f, (deg - 270f) / 90f);
    }
}