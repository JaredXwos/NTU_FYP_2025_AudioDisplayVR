using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ScaleBalance : MonoBehaviour {
    [SerializeField] private float momentOfInertia = 2f;
    [SerializeField, ReadOnly] private Vector3 angularAcceleration = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularVelocity = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularDisplacement = Vector3.zero;
    [SerializeField, ReadOnly] private int weightCount = 0;
    [SerializeField, ReadOnly] private Vector3 pivotOffset;

    private readonly Volatile<Vector3> _angularDisplacement = new(Vector3.zero);

    private readonly HashSet<Weight> weightSet = new();


    #region MonoBehavior
    private void Awake()
    {
        pivotOffset = GetComponentsInChildren<Renderer>()
            .Select(r => r.bounds)
            .Aggregate(
                new Bounds(),
                (overall, b) => { overall.Encapsulate(b); return overall; }
            ).center - transform.position;
    }

    private void FixedUpdate()
    {
        angularAcceleration = weightSet
            .Select(weight => Vector3.Cross(weight.transform.position - transform.position - pivotOffset, Vector3.down * weight.weight))
            .Aggregate(Vector3.zero, (sum, torque) => sum + torque) / momentOfInertia;

        angularVelocity += angularAcceleration * Time.fixedDeltaTime;
        angularDisplacement += 0.5f * angularAcceleration * Time.fixedDeltaTime * Time.fixedDeltaTime + angularVelocity * Time.fixedDeltaTime;
        _angularDisplacement.Value = angularDisplacement;
    }
    #endregion

    public Quaternion GetOrientation => Quaternion.AngleAxis(
        _angularDisplacement.Value.magnitude * Mathf.Rad2Deg,
        _angularDisplacement.Value.normalized
    );

    public void RegisterWeight(Weight weight)
    {
        weightSet.Add(weight);
        ++weightCount;
    }
    public void RegisterWeight(IEnumerable<Weight> weights)
    {
        foreach (Weight weight in weights) weightSet.Add(weight);
        weightCount = weightSet.Count;
    }
    public void RemoveWeight(Weight weight)
    {
        weightSet.Remove(weight);
        --weightCount;
    }
    public void RemoveWeight(IEnumerable<Weight> weights)
    {
        foreach (Weight weight in weights) weightSet.Remove(weight);
        weightCount = weightSet.Count;
    }
    public void ClearWeights()
    {
        weightSet.Clear();
        weightCount = 0;
    }
}