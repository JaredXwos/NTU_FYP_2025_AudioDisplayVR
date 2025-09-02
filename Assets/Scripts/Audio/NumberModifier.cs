using System.Reflection;
using UnityEngine;

/// <summary>
/// Every `intervalSeconds` it jumps the target's public float field `Number`
/// to a random value in [minValue, maxValue], then for the remainder of the
/// interval it either rises or falls at a random gradient (units/sec),
/// clamped to [minValue, maxValue].
/// </summary>
public class NumberModifier : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Component that has a *public float field* named 'Number'.")]
    public Component target;

    [Tooltip("Name of the public float field to drive (default: Number).")]
    public string fieldName = "Number";

    [Header("Value Range")]
    public float minValue = 0f;
    public float maxValue = 100f;

    [Header("Gradient (units per second)")]
    [Tooltip("Minimum absolute gradient applied between jumps.")]
    public float gradientMin = 0.5f;

    [Tooltip("Maximum absolute gradient applied between jumps.")]
    public float gradientMax = 5f;

    [Header("Timing")]
    [Tooltip("Seconds between jumps.")]
    public float intervalSeconds = 20f;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    public bool useUnscaledTime = false;

    [Tooltip("Begin automatically on Start().")]
    public bool autoStart = true;

    // --- internals ---
    FieldInfo _field;
    float _elapsedInInterval;
    float _slopePerSecond; // signed
    bool _holdingAtBoundary; // if we hit min/max, hold until next jump

    void Awake()
    {
        CacheField();
    }

    void OnValidate()
    {
        if (maxValue < minValue) maxValue = minValue;
        if (gradientMin < 0f) gradientMin = 0f;
        if (gradientMax < gradientMin) gradientMax = gradientMin;
        if (intervalSeconds <= 0f) intervalSeconds = 0.01f;
    }

    void Start()
    {
        if (autoStart) BeginNewInterval();
    }

    void Update()
    {
        if (_field == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _elapsedInInterval += dt;

        if (_elapsedInInterval >= intervalSeconds)
        {
            BeginNewInterval();
            return;
        }

        if (_holdingAtBoundary) return;

        float v = GetNumber();

        // If we'd move past a boundary, clamp and hold
        float next = v + _slopePerSecond * dt;
        if (next >= maxValue)
        {
            SetNumber(maxValue);
            _holdingAtBoundary = true;
            return;
        }
        if (next <= minValue)
        {
            SetNumber(minValue);
            _holdingAtBoundary = true;
            return;
        }

        SetNumber(next);
    }

    void BeginNewInterval()
    {
        _elapsedInInterval = 0f;
        _holdingAtBoundary = false;

        // Jump to a random start value within bounds
        float startValue = Random.Range(minValue, maxValue);
        SetNumber(startValue);

        // Choose up or down with equal chance, and a random magnitude
        int dir = Random.value < 0.5f ? -1 : +1;
        float mag = Random.Range(gradientMin, gradientMax);
        _slopePerSecond = dir * mag;
    }

    void CacheField()
    {
        _field = null;
        if (target == null)
        {
            Debug.LogError($"{nameof(NumberModifier)}: No target assigned.", this);
            enabled = false;
            return;
        }

        _field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public);

        if (_field == null || _field.FieldType != typeof(float))
        {
            Debug.LogError($"{nameof(NumberModifier)}: Target {target.GetType().Name} must have a public float field named '{fieldName}'.", this);
            enabled = false;
        }
    }

    float GetNumber()
    {
        return (float)_field.GetValue(target);
    }

    void SetNumber(float v)
    {
        v = Mathf.Clamp(v, minValue, maxValue);
        _field.SetValue(target, v);
    }
}