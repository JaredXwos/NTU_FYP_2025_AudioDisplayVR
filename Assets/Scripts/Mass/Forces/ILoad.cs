using UnityEngine;

public interface ILoad
{
    public Vector3 Force { get; }
    public Vector3 Position { get; }
    public bool enabled { get; set; }
    public GameObject gameObject { get; }
}