using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.UI.Image;

public class ClosestPlaneIntersection : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private int initialRayCount = 360;
    [SerializeField] private int[] raycounts;
    [SerializeField,Range(0.5f,5f)] private float maxRayDistance = 5f;
    [SerializeField] private PolarPoint nearestPoint;

    public PolarPoint NearestPoint => nearestPoint;

    private void Awake()
    {
        rayOrigin = rayOrigin ? rayOrigin : transform.root;
    }

    private void Update()
    {
        if (rayOrigin == null) return;

        Vector3 origin = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;
        Vector3 raycastNormal = rayOrigin.up;

        // 1. Initial 360-degree sweep
        JobHandle handle;
        NativeArray<RaycastHit> initialHits = SweepRaysBidirectional(
            origin,
            direction,
            raycastNormal,
            angleRadians: Mathf.PI * 2f,
            count: initialRayCount / 2,
            maxDistance: maxRayDistance,
            out handle);

        handle.Complete();

        // 2. Prepare initial hit list as a NativeList
        NativeList<NativeArray<RaycastHit>> currentHits =
            new(Allocator.Persistent){ initialHits };

        // 3. Recursive resweeps for each raycount level
        foreach (int rayCount in raycounts)
        {
            // Resweep existing hits
            NativeList<NativeArray<RaycastHit>> nextHits =
                ResweepEachHitBidirectional(
                    currentHits,
                    origin,
                    raycastNormal,
                    rayCount,
                    maxRayDistance);

            // Dispose previous generation arrays
            for (int i = 0; i < currentHits.Length; i++)
                if (currentHits[i].IsCreated)
                    currentHits[i].Dispose();

            // Replace reference with next generation
            currentHits.Dispose();
            currentHits = nextHits;
        }

        // 4. Find closest hit and dispose remaining arrays
        RaycastHit closest = GetClosestHitAndDispose(currentHits);

        // 5. Convert to polar coordinates relative to the sweep plane
        nearestPoint = HitToPolarPoint(origin, raycastNormal, transform.forward,closest);
    }

    public static NativeList<NativeArray<RaycastHit>> ResweepEachHitBidirectional(
        IEnumerable<NativeArray<RaycastHit>> hitBatches,
        Vector3 origin,
        Vector3 raycastNormal,
        int count,
        float maxDistance
        )
    {
        // dynamic container for results
        var secondarySweeps = new NativeList<NativeArray<RaycastHit>>(Allocator.Persistent);

        // count total jobs first (for handle array)
        int totalJobs = hitBatches
            .SelectMany(hits => hits)               // flatten all NativeArrays
            .Count(hit => hit.collider != null);    // count only valid hits

        if (totalJobs == 0)
            return secondarySweeps;

        // Allocate handle array in native memory
        var handles = new NativeArray<JobHandle>(totalJobs, Allocator.Temp);
        int handleIndex = 0;

        // Schedule all resweeps
        foreach (var hits in hitBatches)
        {
            float stepAngle = InferStepAngle(origin, raycastNormal, hits);
            float halfAngle = Mathf.Abs(stepAngle) * 0.5f;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null) continue;

                Vector3 dir = (hit.point - origin).normalized;

                NativeArray<RaycastHit> newHits = SweepRaysBidirectional(
                    origin,
                    dir,
                    raycastNormal,
                    halfAngle,
                    count,
                    maxDistance,
                    out JobHandle handle);

                secondarySweeps.Add(newHits);
                handles[handleIndex++] = handle;
            }

            if (hits.IsCreated)
                hits.Dispose();
        }

        // Combine and complete all raycast jobs
        JobHandle combined = JobHandle.CombineDependencies(handles);
        combined.Complete();

        handles.Dispose();
        return secondarySweeps;
    }

    /// <summary>
    /// Casts (2 * count + 1) rays around 'direction' within 'angleRadians',
    /// rotating around the provided RaycastNormal axis.
    /// Returns a Persistent NativeArray of RaycastHit. Caller must Dispose().
    /// </summary>
    public static NativeArray<RaycastHit> SweepRaysBidirectional(
        Vector3 origin,
        Vector3 direction,
        Vector3 raycastNormal,
        float angleRadians,
        int count,
        float maxDistance,
        out JobHandle handle
        )
    {
        int total = count * 2 + 1;
        var hits = new NativeArray<RaycastHit>(total, Allocator.Persistent);
        var commands = new NativeArray<RaycastCommand>(total, Allocator.TempJob);

        // Normalize input vectors
        direction = direction.normalized;
        raycastNormal = raycastNormal.normalized;

        // Rotation step (split total angle into 'count' parts)
        float step = angleRadians / Mathf.Max(1, count);

        // Shared query settings
        QueryParameters query = new()
        {
            layerMask = ~0, // all layers
            hitTriggers = QueryTriggerInteraction.Collide
        };

        // Center ray
        for (int i = -count; i <= count; i++)
        {
            float angle = step * i;
            Vector3 dir = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, raycastNormal) * direction;
            commands[i + count] = new RaycastCommand(origin, dir, query, maxDistance);
        }

        handle = commands.Dispose(RaycastCommand.ScheduleBatch(commands, hits, 1));

        return hits; // Caller must Dispose() after use
    }

    public static RaycastHit GetClosestHitAndDispose(IEnumerable<NativeArray<RaycastHit>> hitArrays)
    {
        RaycastHit closestHit = default;
        float minDist = float.PositiveInfinity;

        foreach (var hits in hitArrays)
        {
            try
            {
                foreach (var hit in hits)
                    if (hit.collider != null && hit.distance < minDist)
                    {
                        minDist = hit.distance;
                        closestHit = hit;
                    }
            }
            finally
            {
                if (hits.IsCreated)
                    hits.Dispose();
            }
        }

        return closestHit;
    }

    public static RaycastHit GetClosestHitAndDispose(NativeList<NativeArray<RaycastHit>> hitArrays)
    {
        RaycastHit closestHit = default;
        float minDist = float.PositiveInfinity;

        for (int i = 0; i < hitArrays.Length; i++)
        {
            var hits = hitArrays[i];

            try
            {
                for (int j = 0; j < hits.Length; j++)
                {
                    var hit = hits[j];
                    if (hit.collider == null) continue;

                    if (hit.distance < minDist)
                    {
                        minDist = hit.distance;
                        closestHit = hit;
                    }
                }
            }
            finally
            {
                if (hits.IsCreated)
                    hits.Dispose();
            }
        }

        hitArrays.Dispose();
        return closestHit;
    }

    /// <summary>
    /// Converts a RaycastHit to a PolarPoint (angle, radius) relative to a given origin and plane normal.
    /// </summary>
    public static PolarPoint HitToPolarPoint(
    Vector3 origin,
    Vector3 raycastNormal,
    Vector3 referenceDirection,
    RaycastHit hit)
    {
        // Step 1: Compute direction and radius
        Vector3 offset = hit.point - origin;
        float radius = hit.distance; // safer than offset.magnitude

        // Step 2: Project reference direction and offset onto the plane
        Vector3 normal = raycastNormal.normalized;
        Vector3 refDir = Vector3.ProjectOnPlane(referenceDirection, normal).normalized;
        Vector3 hitDir = Vector3.ProjectOnPlane(offset, normal).normalized;

        // Step 3: Compute signed angle around the plane normal
        // Unity defines positive as counter-clockwise when looking along 'normal'
        float angleCCW = Mathf.Atan2(
            Vector3.Dot(Vector3.Cross(refDir, hitDir), normal),
            Vector3.Dot(refDir, hitDir)
        );

        // Step 4: Convert to clockwise convention (invert sign)
        float angleCW = -angleCCW;

        // Clamp to [-pi, pi]
        if (angleCW > Mathf.PI) angleCW -= 2f * Mathf.PI;
        else if (angleCW < -Mathf.PI) angleCW += 2f * Mathf.PI;

        return new PolarPoint(radius, angleCW);
    }

    public static float InferStepAngle(Vector3 origin, Vector3 raycastNormal, NativeArray<RaycastHit> hits)
    {
        // Find two valid hits
        int i1 = -1, i2 = -1;
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].collider != null)
            {
                if (i1 == -1) i1 = i;
                else { i2 = i; break; }
            }

        // Not enough hits to infer anything
        if (i1 == -1 || i2 == -1)
            return 0f;

        // Reconstruct directions from origin to hit points
        Vector3 dir1 = (hits[i1].point - origin).normalized;
        Vector3 dir2 = (hits[i2].point - origin).normalized;

        // Compute signed angular difference around the plane normal
        return Vector3.SignedAngle(dir1, dir2, raycastNormal) * Mathf.Deg2Rad;
    }
}