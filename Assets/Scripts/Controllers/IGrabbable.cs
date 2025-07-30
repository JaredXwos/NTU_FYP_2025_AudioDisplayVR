using UnityEngine;

public interface IGrabbable : ILimitedAccess
{
    public void SetTransform(Vector3? position, int? orientation);
    public bool CanBeMoved { get; }
    public int Orientation { get; }
    public Vector3 Position { get; }

    public void SetCanBeMoved(object Key, bool canBeMoved);
}