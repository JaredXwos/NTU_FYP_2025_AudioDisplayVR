using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IHasObstacleSignal
{
    ObstacleSignal ObstacleSignal { get; }
}

public readonly struct ObstacleSignal
{
    public readonly float distance;
    public readonly float[] directions;

    public ObstacleSignal(float distance, float[] directions)
    {
        if (directions == null)
            throw new ArgumentException("Directions array invalid", nameof(directions));

        foreach (var dir in directions)
            if (dir < -Mathf.PI || dir > Mathf.PI)
                throw new System.ArgumentOutOfRangeException(nameof(directions),
                    "Direction values must be in range [-pi, pi]");

        this.distance = distance;
        this.directions = directions;

        Array.Sort(this.directions); // ensure invariant
    }

    public static ObstacleSignal Empty => new(float.MaxValue, Array.Empty<float>());

    public override string ToString()
    {
        return $"ObstacleSignal(distance: {distance}, directions: [{string.Join(", ", directions)}])";
    }

    public ObstacleSignal FirstPassReduce(float precision)
    {
        if (directions.Length < 2 || precision <= 0f)
            return this;

        float[] dirs = directions;
        List<float> reduced = new();


        float start = float.NaN;

        int index = -1;
        float previous, current = dirs[^1];

        while (true)
        {
            previous = current;
            current = dirs[(++index + dirs.Length) % dirs.Length];

            if (index >=  dirs.Length * 2)
            {
                reduced = new() { -Mathf.PI / 2, 0, Mathf.PI / 2, Mathf.PI };
                break;
            }

            if (Anglef.CounterClockwiseAngle(previous, current) >= precision)
            {
                if (!float.IsNaN(start)) // close previous group if we know it
                {
                    float sweep = Anglef.CounterClockwiseAngle(start, previous);
                    if (sweep == 0f) reduced.Add(start);
                    else
                    {
                        int segmentCount = (int)Mathf.Ceil(sweep / precision);
                        float segmentHalf = sweep / segmentCount / 2f;
                        for (int s = 0; s < segmentCount; s++)
                            reduced.Add(Anglef.Normalize(start + segmentHalf * (2 * s + 1)));
                    }
                }

                if (index >= dirs.Length) break; // completed full loop
                start = current;
            }
        }

        return new ObstacleSignal(distance, reduced.ToArray());
    }

    public ObstacleSignal MergeClosestPair()
    {
        float[] dirs = directions;
        int len = dirs.Length;

        if (len < 2)
            return this;

        // find smallest adjacent diff
        int minIndex = 1;
        float minDiff = dirs[1] - dirs[0];

        for (int i = 2; i < len; i++)
        {
            float diff = dirs[i] - dirs[i - 1];
            if (diff < minDiff)
            {
                minDiff = diff;
                minIndex = i;
            }
        }

        // check wraparound diff (last to first)
        if (dirs[0] + 2f * Mathf.PI - dirs[len - 1] < minDiff)
            minIndex = 0;

        // compute merge
        int prev = (minIndex - 1 + len) % len;
        float merged = 0.5f * (dirs[minIndex] + dirs[prev]); // arithmetic mean

        // mark deletion
        dirs[prev] = float.NaN;
        dirs[minIndex] = merged;

        return new ObstacleSignal(distance, dirs.Where(d => !float.IsNaN(d)).ToArray());
    }

    public ObstacleSignal ReduceToCount(int targetCount, float precision)
    {
        if (targetCount == 0)
            throw new ArgumentException("targetCount must be greater than zero", nameof(targetCount));

        ObstacleSignal sig = FirstPassReduce(precision);

        while (sig.directions.Length > targetCount)
            sig = sig.MergeClosestPair();

        return sig;
    }
}