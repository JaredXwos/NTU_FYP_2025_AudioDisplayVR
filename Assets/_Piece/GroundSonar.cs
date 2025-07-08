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
public class GroundSonar : MonoBehaviour
{
    [SerializeField, ReadOnly] private int[] groundClearance = new int[3] {-1, -1, -1};
    [SerializeField] private bool broadcastFitEvent = true;

    private readonly Volatile<int[]> _groundClearance = new(new int[3]);
    public CoreComponent Parent { get; private set; }
    private IEnumerable<Transform> ComponentTransforms;
    private Delegate invoke;

    #region MonoBehavior
    private void Start()
    {
        Parent = GetComponent<CoreComponent>();
        ComponentTransforms = GetComponentsInChildren<Transform>().Where(t => t.gameObject.GetComponent<Collider>() != null);
        Debug.Log($"Component Parts: {ComponentTransforms.Count()}");
        MonoBehaviour[] listeners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(mb =>
            mb.GetType().GetInterfaces().Any(iface =>
                typeof(IHas<>).MakeGenericType(typeof(FitEventHandler<>).MakeGenericType(Parent.GetType())).IsAssignableFrom(iface)
            ))
            .ToArray();

        object broadcast = Activator.CreateInstance(
            typeof(EventBroadcaster<,>).MakeGenericType(
                typeof(IHas<>).MakeGenericType(
                    typeof(FitEventHandler<>).MakeGenericType(Parent.GetType())
                ),
                typeof(ValueTuple<,>).MakeGenericType(Parent.GetType(), typeof(GameObject))
            ),
            new object[] { listeners } // forces single array param
        );
        Type payloadType = typeof(ValueTuple<,>).MakeGenericType(Parent.GetType(), typeof(GameObject));
        Type delegateType = typeof(Action<>).MakeGenericType(payloadType);

        System.Reflection.MethodInfo method = broadcast.GetType().GetMethod("InvokeEvent");
        if (method == null)
        {   
            Debug.LogError("[Ground Sensor] Reflection type error with broadcaster");
        }
        else invoke = Delegate.CreateDelegate(delegateType, broadcast, method);
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
            if (Physics.Raycast(startPoint[i], downward, out RaycastHit hit, 10f))
            {
                groundClearance[i] = Mathf.FloorToInt(hit.distance);
                collided = hit.transform.root.gameObject;
            }
            else
            {
                Array.Fill(groundClearance, -1);
                break;
            }

        _groundClearance.Value = (int[])groundClearance.Clone();

        if (broadcastFitEvent && collided != null && groundClearance.All(h => h == 0)) 
            invoke.DynamicInvoke(Activator.CreateInstance(
                typeof(ValueTuple<,>).MakeGenericType(Parent.GetType(), typeof(GameObject)),
                Parent, 
                collided
            ));
    }
    #endregion

    public int[] GetGroundClearance() => _groundClearance.Value;
}