using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ClosestObstacleAngleDistanceInputProvider : MonoBehaviour, IHasAngle, IHasDistance
{
    [SerializeField] ClosestPlaneIntersection closestPlaneIntersection;

    private void Awake()
    {
        Check.PropertyEnabledElseAssign<ClosestPlaneIntersection>(this, "closestPlaneIntersection");
    }

    public float Angle => -closestPlaneIntersection.NearestPoint.Radians;

    public float Distance => closestPlaneIntersection.NearestPoint.Radius;
}