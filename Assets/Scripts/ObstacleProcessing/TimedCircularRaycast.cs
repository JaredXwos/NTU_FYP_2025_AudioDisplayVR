using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TimedCircularRaycast : MonoBehaviour
{
    [Header("Raycast Timing")]
    [Tooltip("Seconds per full 360° sweep. 0.1 = 10 Hz")]
    [Min(0.001f)] public float sweepPeriod = 0.1f;

    [Header("Ray Parameters")]
    [Min(8)] public int raysPerRevolution = 9400;
    public float maxDistance = 0.2f;

    public NativeArray<RaycastHit> LatestSweepHits => _frontResults;

    private float _timeAccum;

    private Vector3[] _rayDirections;
    private NativeArray<RaycastCommand> _commands;
    private NativeArray<RaycastHit> _frontResults;
    private NativeArray<RaycastHit> _backResults;

    private JobHandle _jobHandle;
    private bool _jobRunning;

    private void OnEnable()
    {
        _rayDirections = new Vector3[raysPerRevolution];
        float step = 360f / raysPerRevolution;
        for (int i = 0; i < raysPerRevolution; i++)
        {
            float yaw = step * i;
            _rayDirections[i] = Quaternion.AngleAxis(yaw, Vector3.up) * Vector3.forward;
        }

        _timeAccum = 0f;

        _commands = new NativeArray<RaycastCommand>(raysPerRevolution, Allocator.Persistent);
        _frontResults = new NativeArray<RaycastHit>(raysPerRevolution, Allocator.Persistent);
        _backResults = new NativeArray<RaycastHit>(raysPerRevolution, Allocator.Persistent);

        ScheduleSweep(); // start immediately
    }

    private void OnDisable()
    {
        if (_jobRunning)
        {
            _jobHandle.Complete();
            _jobRunning = false;
        }

        if (_commands.IsCreated) _commands.Dispose();
        if (_frontResults.IsCreated) _frontResults.Dispose();
        if (_backResults.IsCreated) _backResults.Dispose();
    }

    private void Update()
    {
        if (_jobRunning && _jobHandle.IsCompleted)
        {
            _jobHandle.Complete();
            _jobRunning = false;
            (_backResults, _frontResults) = (_frontResults, _backResults); // newest results now in _frontResults
        }

        _timeAccum += Time.deltaTime;
        while (_timeAccum >= sweepPeriod)
        {
            _timeAccum -= sweepPeriod;
            if (!_jobRunning)
                ScheduleSweep();
        }
        var hasHit = System.Array.FindIndex(_frontResults.ToArray(), h => h.collider != null) is var i && i >= 0;
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if (_rayDirections == null || _rayDirections.Length == 0) return;
        if (!_frontResults.IsCreated) return;

        transform.GetPositionAndRotation(out Vector3 origin, out Quaternion rot);

        const float segmentLength = 0.05f;
        const int skipRate = 5;              

        for (int i = 0; i < raysPerRevolution; i++)
        {
            bool hit = _frontResults[i].collider != null;
            Gizmos.color = hit ? Color.green : Color.red;
            Vector3 segVec = rot * _rayDirections[i] * segmentLength;

            Vector3 start = origin;
            for (int s = 0; s < Mathf.CeilToInt((hit ? _frontResults[i].distance : maxDistance) / segmentLength); s++)
            {
                Vector3 end = start + segVec;

                // dotted pattern: only draw every (skipRate+1)th segment
                if (s % (skipRate + 1) == 0)
                    Gizmos.DrawLine(start, end);

                start = end;
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(origin, 0.03f);
    }
    private void ScheduleSweep()
    {
        transform.GetPositionAndRotation(out Vector3 origin, out Quaternion rot);

        // Fully qualify to avoid DOTS-vs-Engine mixups.
        var query = new QueryParameters
        {
            layerMask = ~0,        // everything
            hitTriggers = QueryTriggerInteraction.Collide,     // set true if you need triggers
            hitBackfaces = false,
            hitMultipleFaces = false
        };

        for (int i = 0; i < raysPerRevolution; i++)
        {
            Vector3 dir = rot * _rayDirections[i];

            _commands[i] = new RaycastCommand(
                origin,
                dir,
                query,
                maxDistance
            );
        }

        _jobHandle = RaycastCommand.ScheduleBatch(_commands, _backResults, 256);
        _jobRunning = true;
    }
}