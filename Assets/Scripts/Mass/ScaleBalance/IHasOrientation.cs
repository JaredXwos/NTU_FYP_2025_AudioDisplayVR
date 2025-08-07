using UnityEngine;

public interface IHasOrientation
{
    /// <summary>
    /// Gets the orientation of the object.
    /// </summary>
    Vector3 Orientation { get; }
    /// <summary>
    /// Gets the orientation as a Quaternion.
    /// </summary>
    Quaternion GetOrientation { get; }
}