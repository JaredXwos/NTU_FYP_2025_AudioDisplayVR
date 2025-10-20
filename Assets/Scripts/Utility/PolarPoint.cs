using UnityEngine;

public readonly struct PolarPoint
{
    public readonly float Radius;
    public readonly float Radians;
    public PolarPoint(float radius, float radians)
    {
        Radius = radius;
        Radians = radians;
    }

    public override string ToString()
    {
        return $"(r={Radius:F3}, theta={Radians*Mathf.Rad2Deg:F3} deg)";
    }
}