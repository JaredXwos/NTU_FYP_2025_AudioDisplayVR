using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ObstacleProcessor
{
    private readonly float distancePrecision;
    private readonly float directionPrecision;
    private readonly int maxDirections;

    public ObstacleProcessor(float distancePrecision = 0.05f, float directionPrecision = 0, int maxDirections = 4)
    {
        this.distancePrecision = distancePrecision;
        this.directionPrecision = directionPrecision;
        this.maxDirections = maxDirections;
    }
    public ObstacleSignal Process(
        NativeArray<RaycastHit> hits, 
        Vector3 origin, Vector3 normal, Vector3 referenceDirection
        )
    {
        if (referenceDirection == Vector3.zero)
            throw new ArgumentException("referenceDirection cannot be zero");

        if (normal == Vector3.zero)
            throw new ArgumentException("normal cannot be zero");

        if (hits.Length == 0)
            return ObstacleSignal.Empty;

        return
            GetClosestObstacleSignal(hits, origin, normal, referenceDirection).
            ReduceToCount(maxDirections, directionPrecision);
    }

    public ObstacleSignal GetClosestObstacleSignal(
    NativeArray<RaycastHit> hits,
    Vector3 origin, Vector3 normal, Vector3 referenceDirection)
    {
        // Pass 0: count valid hits
        int validCount = 0;
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].collider != null && hits[i].distance > 0f)
                validCount++;

        // No valid hits  return "no obstacle"
        if (validCount == 0)
        {
            // You can choose any sentinel; PositiveInfinity is convenient
            return new ObstacleSignal(float.PositiveInfinity, System.Array.Empty<float>());
        }

        // Pass 1: find true minimum over valid hits only
        float minDistance = float.PositiveInfinity;
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            float d = h.distance;
            if (d <= 0f) continue;

            if (d < minDistance) minDistance = d;
        }

        // Pass 2: collect angles for hits within the precision band around the min
        // (again only for valid hits)
        List<float> directions = new();
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            float d = h.distance;
            if (d <= 0f) continue;

            if (Mathf.Abs(d - minDistance) <= distancePrecision)
                directions.Add(GetAngle(h, origin, normal, referenceDirection));
        }

        var result = new ObstacleSignal(minDistance, directions.ToArray());
        // Debug (optional)
        // Debug.Log($"Valid hits: {validCount}, Min: {minDistance}, DirCount: {directions.Count}");
        return result;
    }

    public static float GetAngle(RaycastHit hit, Vector3 origin, Vector3 normal, Vector3 referenceDirection)
    {
        Vector3 a = Vector3.ProjectOnPlane(referenceDirection, normal).normalized;
        Vector3 b = Vector3.ProjectOnPlane(hit.point - origin, normal).normalized;

        return Mathf.Atan2(
            Vector3.Dot(Vector3.Cross(a, b), normal),
            Vector3.Dot(a, b));
    }
}