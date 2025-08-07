using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(CollidingComponent))]
public abstract class AbstractScaleBalance : MonoBehaviour
{
    [SerializeField, ReadOnly] protected GameObjectInt[] torques;
    [SerializeField, ReadOnly] protected int weightCount = 0;
    [SerializeField, ReadOnly] protected Vector3 origin;

    [SerializeField] protected CollidingComponent Parent;

    protected readonly HashSet<ILoad> weightSet = new();


    #region MonoBehavior
    protected virtual void Awake()
    {
        Check.ForLocalComponentAndDisable<ScaleBalance>(this);
        Check.PropertyEnabledElseAssign<CollidingComponent>(this, "Parent");
    }

    protected virtual void Start()
    {
        HashSet<Vector3Int> body = Parent.GetBody();
        origin = new(
            body.Max(v => v.x) + body.Min(v => v.x),
            body.Max(v => v.y) + body.Min(v => v.y),
            body.Max(v => v.z) + body.Min(v => v.z)
        );
        origin /= 2f;
    }

    protected virtual void Update() => weightSet.RemoveWhere(mb => !mb.enabled);

    protected virtual void OnDrawGizmos()
    {
        if (weightSet == null) return;

        Gizmos.color = Color.magenta;

        foreach (var weight in weightSet)
        {
            if (!weight.enabled) continue;

            Vector3 start = weight.Position;
            Vector3 end = start + weight.Force;

            Gizmos.DrawLine(start, end);

            Gizmos.DrawSphere(start, 0.02f);

            Gizmos.DrawSphere(end, 0.01f);
        }
    }
    #endregion

    public void RegisterWeight(ILoad weight)
    {
        if(weightSet.Add(weight))
            ++weightCount;
    }
    public void RegisterWeight(IEnumerable<ILoad> weights)
    {
        foreach (ILoad weight in weights) weightSet.Add(weight);
        weightCount = weightSet.Count;
    }
    public void RemoveWeight(ILoad weight)
    {
        if(weightSet.Remove(weight))
            --weightCount;
    }
    public void RemoveWeight(IEnumerable<ILoad> weights)
    {
        foreach (ILoad weight in weights) weightSet.Remove(weight);
        weightCount = weightSet.Count;
    }
    public void ClearWeights()
    {
        weightSet.Clear();
        weightCount = 0;
    }
}