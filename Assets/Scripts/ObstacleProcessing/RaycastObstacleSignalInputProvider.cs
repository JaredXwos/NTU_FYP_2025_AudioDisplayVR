using System;
using UnityEngine;

public class RaycastObstacleSignalInputProvider : MonoBehaviour, IHasObstacleSignal
{
    [SerializeField, Range(0, 5)] private float distancePrecision = 0.5f;
    [SerializeField, Range(0, Mathf.PI*2)] private float directionPrecision = 0.1f;
    [SerializeField, Range(1, 10)] private int maxDirections = 4;


    [SerializeField] private TimedCircularRaycast Raycaster;
    [SerializeField] private Transform OriginTransform;
    [SerializeField] private float[] Directions;
    private ObstacleProcessor Processor;
    
    private ObstacleSignal latestSignal = ObstacleSignal.Empty;
    public ObstacleSignal ObstacleSignal => latestSignal;
    private void Awake()
    {
        Check.PropertyEnabledElseAssign<TimedCircularRaycast>(this, "Raycaster");
        Processor = new(distancePrecision, directionPrecision, maxDirections);
        if (OriginTransform == null)
            OriginTransform = transform.root;
    }

    private void Update()
    {
        latestSignal = Processor.Process(
            Raycaster.LatestSweepHits,
            OriginTransform.position, OriginTransform.up, OriginTransform.forward
            );
        Directions = latestSignal.directions;
    }
        
    
    private void OnDrawGizmosSelected()
    {
        if (latestSignal.directions == null || latestSignal.directions.Length == 0)
            return;

        Vector3 origin = OriginTransform != null ? OriginTransform.position : transform.position;
        Vector3 normal = OriginTransform != null ? OriginTransform.up : transform.up;
        Vector3 referenceDir = OriginTransform != null ? OriginTransform.forward : transform.forward;

        Gizmos.color = Color.yellow;

        foreach (float angleRad in latestSignal.directions)
        {
            // Convert angle around normal into a world-space direction
            Vector3 endPoint = origin + 
                Quaternion.AngleAxis(angleRad * Mathf.Rad2Deg, normal) * referenceDir * latestSignal.distance;

            Gizmos.DrawLine(origin, endPoint);
            Gizmos.DrawSphere(endPoint, 0.03f);
        }
    }
}