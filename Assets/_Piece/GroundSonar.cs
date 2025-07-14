using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class FitEventHandler<collider> : Handler<(collider, GameObject)> where collider : CoreComponent
{
    public FitEventHandler(Action<(collider, GameObject)> handler) : base(handler) {}
}

[RequireComponent(typeof(CoreComponent))]
public class GroundSonar : Dispatch
{
    [SerializeField, ReadOnly] private int[] groundClearance = new int[3] {-1, -1, -1};
    [SerializeField] private bool broadcastFitEvent = true;
    [SerializeField, ReadOnly] private bool isCurrentlyFit = false;

    private readonly Volatile<int[]> _groundClearance = new(new int[3]);
    public CoreComponent Parent { get; private set; }

    private IEnumerable<Transform> ComponentTransforms;

    protected override void Awake()
    {
        Parent = GetComponent<CoreComponent>();
        HandlerType = typeof(FitEventHandler<>).MakeGenericType(Parent.GetType());
        PayloadType = typeof(ValueTuple<,>).MakeGenericType(Parent.GetType(), typeof(GameObject));
        base.Awake();
    }
    #region MonoBehavior
    protected override void Start()
    {
        base.Start();
        ComponentTransforms = GetComponentsInChildren<Transform>().Where(t => t.gameObject.GetComponent<Collider>() != null);
    }

    protected void Update()
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
            Invoke(Activator.CreateInstance(PayloadType, new object[] { Parent, collided }));
        
        isCurrentlyFit = collided != null && groundClearance.All(h => h == 0);
    }
    #endregion
    protected override Type HandlerType { get; set; }

    protected override Type PayloadType { get; set; }

    public int[] GetGroundClearance() => _groundClearance.Value;
}