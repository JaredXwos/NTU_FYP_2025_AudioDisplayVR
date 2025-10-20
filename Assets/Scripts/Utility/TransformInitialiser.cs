using UnityEngine;

/// <summary>
/// ScriptableObject that stores Transform initialization data.
/// </summary>
[CreateAssetMenu(fileName = "NewTransformInitializer", menuName = "Initialization/Transform Initializer")]
public class TransformInitializer : ScriptableObject
{
    [Header("Transform Data")]
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero; // Stored in Euler angles
    public Vector3 scale = Vector3.one;

    /// <summary>
    /// Applies this transform data to a target Transform.
    /// </summary>
    /// <param name="target">Transform to apply data to.</param>
    public void ApplyTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[{name}] Tried to apply TransformInitializer to a null target.");
            return;
        }

        target.position = position;
        target.rotation = Quaternion.Euler(rotation);
        target.localScale = scale;
    }

    /// <summary>
    /// Captures the given transform's current data into this ScriptableObject.
    /// (Useful for saving presets directly from the Editor)
    /// </summary>
    /// <param name="source">Transform to copy data from.</param>
    public void CaptureFrom(Transform source)
    {
        if (source == null)
        {
            Debug.LogWarning($"[{name}] Tried to capture Transform data from a null source.");
            return;
        }

        position = source.position;
        rotation = source.rotation.eulerAngles;
        scale = source.localScale;
    }
}