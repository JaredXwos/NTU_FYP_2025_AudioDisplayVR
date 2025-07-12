using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
    [SerializeField] private float weightPerUnitCubeFluid = 1000f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField, ReadOnly] private float radius;
    [SerializeField, ReadOnly] private Vector3 origin;
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
        Bounds body = bounds
            .Aggregate(
                bounds.First(),
                (overall, b) => { overall.Encapsulate(b); return overall; }
            );
        radius = body.extents.magnitude;
        origin = body.center;
    }
    private void Update()
    {
        Vector3 angleAxisVector = scale.Orientation;

        centreOfBuoyancy.Value = CalculateCentreOfBuoyancy(scale.GetOrientation) + origin;

        float turnAngle = angleAxisVector.magnitude % (2 * Mathf.PI);
        angleAxisVector = angleAxisVector.normalized * turnAngle;
        if (turnAngle > Mathf.PI)
        {
            turnAngle = 2 * Mathf.PI - turnAngle;
            angleAxisVector = -angleAxisVector.normalized * turnAngle;
        }
        if (turnAngle <= 0) return;

        if (centreOfBuoyancy.Value.z != origin.z) Debug.LogError($"[Buoyancy Error Detected] ({centreOfBuoyancy.Value.x:G9}, {centreOfBuoyancy.Value.y:G9}, {centreOfBuoyancy.Value.z:G9})");
        buoyantForceMagnitude.Value = 2f / 3f * Mathf.PI * radius * radius * radius * turnAngle * weightPerUnitCubeFluid;

        CentreOfBuoyancy = centreOfBuoyancy.Value;
        BuoyantForceMagnitude = buoyantForceMagnitude.Value;
    }   
    #endregion

    #region ILoad
    public Vector3 Position => centreOfBuoyancy.Value;
    public Vector3 Force => buoyantForceMagnitude.Value * Vector3.up;
    #endregion

    private Vector3 CalculateCentreOfBuoyancy(Quaternion orientation)
    {
        orientation.ToAngleAxis(out float angleDegrees, out Vector3 axis);

        if (angleDegrees == 0f) return Vector3.zero;

        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float halfAngle = 0.5f * angleRadians;

        Vector3 intersection = Vector3.Cross(Vector3.down, axis);

        Vector3 centroidDirection
            = Quaternion.AngleAxis(halfAngle * Mathf.Rad2Deg, axis) * intersection.normalized;

        float displacement = (4 * radius * Mathf.Sin(halfAngle)) / (3 * halfAngle);

        Vector3 centroid = centroidDirection * displacement;

        return centroid;
    }

    private static string Vec3(Vector3 v)
=> $"({v.x:R}, {v.y:R}, {v.z:R})";
}