using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ScaleBalance : MonoBehaviour {
    [SerializeField] private float momentOfInertia = 2f;
    [SerializeField, ReadOnly] private Vector3 angularAcceleration = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularVelocity = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularDisplacement = Vector3.zero;
    [SerializeField, ReadOnly] private GameObjectInt[] torques; 
    [SerializeField, ReadOnly] private int weightCount = 0;
    [SerializeField, ReadOnly] private Vector3 origin;

    private readonly Volatile<Vector3> _angularDisplacement = new(Vector3.zero);

    private readonly HashSet<ILoad> weightSet = new();


    #region MonoBehavior
    private void Awake()
    {
        Check.ForLocalComponentAndDisable<ScaleBalance>(this);
        origin = GetComponentsInChildren<Renderer>()
            .Select(r => r.bounds)
            .Aggregate(
                new Bounds(),
                (overall, b) => { overall.Encapsulate(b); return overall; }
            ).center;
    }

    private void Update() => weightSet.RemoveWhere(mb => !mb.enabled);

    private void FixedUpdate()
    {
        weightSet.RemoveWhere(load => (UnityEngine.Object)load == null);
        torques = weightSet
            .Select(weight => new GameObjectInt { gameObject = weight.gameObject, value = (int) Vector3.Cross(weight.Position - origin, weight.Force).magnitude })
            .ToArray();
        angularDisplacement += 0.5f * Time.fixedDeltaTime * Time.fixedDeltaTime * angularAcceleration + angularVelocity * Time.fixedDeltaTime;

        angularVelocity += 0.1f * Time.fixedDeltaTime * angularAcceleration;
        angularAcceleration = weightSet
            .Select(weight =>
            {
                Vector3 r = weight.Position - transform.position - origin;
                Vector3 F = weight.Force;

                Vector3 torque = Vector3.Cross(r, F);

                return torque;
            })
            .Aggregate(Vector3.zero, (sum, torque) => sum + torque);

        angularAcceleration /= momentOfInertia;

        _angularDisplacement.Value = angularDisplacement;
    }

    private void OnDrawGizmos()
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

    public Vector3 Orientation => _angularDisplacement.Value;
    public Quaternion GetOrientation => Quaternion.AngleAxis(
        _angularDisplacement.Value.magnitude * Mathf.Rad2Deg,
        _angularDisplacement.Value.normalized);

    public void RegisterWeight(ILoad weight)
    {
        weightSet.Add(weight);
        ++weightCount;
    }
    public void RegisterWeight(IEnumerable<ILoad> weights)
    {
        foreach (ILoad weight in weights) weightSet.Add(weight);
        weightCount = weightSet.Count;
    }
    public void RemoveWeight(ILoad weight)
    {
        weightSet.Remove(weight);
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