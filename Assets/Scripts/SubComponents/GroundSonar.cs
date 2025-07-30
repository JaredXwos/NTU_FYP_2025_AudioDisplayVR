using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public record FitEventPayload : EventPayload, IPParentCoreComponent, IPActive, IPCollidee
{
    public CoreComponent Parent { get; }
    public GameObject Collidee { get; }
    public bool IsActive { get; }
    public FitEventPayload(CoreComponent parent, GameObject collidee, bool isActive){
        Parent = parent;
        IsActive = isActive;
        Collidee = collidee;    
    }
}

[RequireComponent(typeof(CoreComponent))]
public class GroundSonar : Dispatch
{
    [SerializeField, ReadOnly] protected int[] groundClearance = new int[3] {-1, -1, -1};
    [SerializeField] protected bool broadcastFitEvent = true;
    [SerializeField, ReadOnly] protected bool isCurrentlyFit = false;

    protected readonly Volatile<int[]> _groundClearance = new(new int[3]);
    public CoreComponent Parent { get; protected set; }

    protected IEnumerable<Transform> ComponentTransforms;

    #region MonoBehavior
    protected override void Awake()
    {
        Parent = GetComponent<CoreComponent>();
        EventType = typeof(FitEvent);
        PayloadType = typeof(FitEventPayload);
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        ComponentTransforms = GetComponentsInChildren<Transform>().Where(t => t.gameObject.GetComponent<Collider>() != null);
    }
    protected virtual void Update()
    {
        Vector3 downward = transform.TransformDirection(Vector3.down);
        
        Vector3[] startPoint = ComponentTransforms
            .Select(p => new Vector3(p.position.x, p.position.y - p.localScale.y / 2, p.position.z) - downward * 0.1f)
            .OrderBy(x => x.x)
            .ThenBy(x => x.z)
            .ToArray();
        GameObject collided = null;
        for (int i = 0; i < startPoint.Length; i++)
            if (Physics.Raycast(startPoint[i], downward, out RaycastHit hit, 10f) && hit.transform.root.gameObject != gameObject)
            {
                groundClearance[i] = Mathf.FloorToInt(hit.distance);
                collided = hit.transform.root.gameObject;
            }
            else
            {
                collided = null;
                Array.Fill(groundClearance, -1);
                break;
            }

        _groundClearance.Value = (int[])groundClearance.Clone();

        
        if (broadcastFitEvent && collided != null && groundClearance.All(h => h == 0) && !isCurrentlyFit)
            Invoke(new FitEventPayload(Parent, collided, true));
        
        isCurrentlyFit = collided != null && groundClearance.All(h => h == 0);
    }
    #endregion

    public int[] GetGroundClearance() => _groundClearance.Value;
}