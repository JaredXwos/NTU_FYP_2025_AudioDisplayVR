using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// This class calculates a bouyancy force.
/// 
/// It envisions the scale as a hemisphere of some radius just above the surface of the fluid.
/// The force of bouyancy acts on the centroid of the displaced fluid, with the force of the mass of the displaced fluid.
/// 
/// As the scale tilts, the displaced fluid shape will be what is known as a spherical wedge (something like a watermelon slice)
/// The wedge is bounded by the sphere of radius radius, y = 0 (the original fluid level) and the flat bottom plane of the hemisphere given by rotating y = 0 by some rotation.
/// 
/// The plane perpedicular to the axis of rotation is called the median symmetry plane, and cuts the spherical wedge neatly into quarters.
/// 
/// </summary>
[RequireComponent(typeof(ScaleBalance))]
public class Bouyancy : MonoBehaviour, ILoad
{

    [SerializeField] private float mass;
    [SerializeField, ReadOnly] private float radius;
    [SerializeField, ReadOnly] private Vector3 CentreOfBuoyancy;
    [SerializeField, ReadOnly] private float BuoyantForceMagnitude;

    private ScaleBalance scale;
    private readonly Volatile<float> buoyantForceMagnitude = new(0);
    private readonly Volatile<Vector3> centreOfBuoyancy = new(Vector3.zero);

    #region MonoBehaviour
    private void Awake()
    {
        scale = GetComponent<ScaleBalance>();
        scale.RegisterWeight(this);

    }
    private void Start()
    {
        Debug.LogFormat($"Children: {transform.GetComponentsInChildren<Transform>().Length}");
        if (GetComponentsInChildren<Renderer>().Length == 0) Debug.LogWarning("No renderers found");
        IEnumerable<Bounds> bounds = GetComponentsInChildren<Renderer>()
            .Select(r => r.bounds);
        radius = bounds
            .Aggregate(
                bounds.First(),
                (overall, b) => { overall.Encapsulate(b); return overall; }
            ).extents.magnitude;
    }
    private void Update()
    {
        scale.GetOrientation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle == 0f) return;

        float halfAngle = 0.5f * angle * Mathf.Deg2Rad;
        Vector3 intersection = Vector3.Cross(Vector3.down, axis);
        Vector3 centroidDirection = Quaternion.AngleAxis(halfAngle * Mathf.Rad2Deg, axis) * intersection.normalized;

        float displacement = (4 * radius * Mathf.Sin(halfAngle)) / (3 * halfAngle);
        centreOfBuoyancy.Value = transform.position + centroidDirection * displacement;

        buoyantForceMagnitude.Value = 2f / 3f * Mathf.PI * Mathf.Pow(radius, 3) * angle * mass;

        CentreOfBuoyancy = centreOfBuoyancy.Value;
        BuoyantForceMagnitude = buoyantForceMagnitude.Value;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(centreOfBuoyancy.Value, 0.05f);
        Gizmos.DrawLine(centreOfBuoyancy.Value, centreOfBuoyancy.Value + Force * 0.1f);
    }
    #endregion

    #region ILoad
    public Vector3 Position => centreOfBuoyancy.Value;
    public Vector3 Force => buoyantForceMagnitude.Value * Vector3.up;
    #endregion
}