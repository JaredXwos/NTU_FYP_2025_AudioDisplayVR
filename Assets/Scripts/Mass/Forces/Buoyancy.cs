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
[RequireComponent(typeof(CollidingComponent))]
public class Bouyancy : MonoBehaviour, ILoad
{
    [SerializeField] private float weightPerUnitCubeFluid;
    [SerializeField, ReadOnly] private float radius;
    [SerializeField, ReadOnly] private Vector3 origin;
    [SerializeField, ReadOnly] private Vector3 CentreOfBuoyancy;
    [SerializeField, ReadOnly] private float BuoyantForceMagnitude;
    [SerializeField, ReadOnly] private float MaximalApplicableTorque;

    private ScaleBalance scale;
    private CollidingComponent Parent;
    private readonly Volatile<float> buoyantForceMagnitude = new(0);
    private readonly Volatile<Vector3> centreOfBuoyancy = new(Vector3.zero);

    #region MonoBehaviour
    private void Awake()
    {
        Check.PropertyEnabledElseAssign<ScaleBalance>(this, "scale");
        Check.PropertyEnabledElseAssign<CollidingComponent>(this, "Parent");
        scale.RegisterWeight(this);
    }
    private void Start()
    {
        HashSet<Vector3Int> body = Parent.GetBody();
        origin = new(
            body.Max(v => v.x) + body.Min(v => v.x),
            body.Max(v => v.y) + body.Min(v => v.y),
            body.Max(v => v.z) + body.Min(v => v.z)
        );
        origin /= 2f;
        radius = body.Max(v => Mathf.Abs((v - origin).magnitude));

        Quaternion test = new(0.707f, 0.707f, 0, 0);
        MaximalApplicableTorque = Vector3.Cross(CalculateCentreOfBuoyancy(test), Vector3.up * CalculateMagnitudeOfBuoyantForce(test)).magnitude / 2;
    }
    private void Update()
    {
        Vector3 angleAxisVector = scale.Orientation;

        centreOfBuoyancy.Value = CalculateCentreOfBuoyancy(scale.GetOrientation) + origin;

        buoyantForceMagnitude.Value = CalculateMagnitudeOfBuoyantForce(scale.GetOrientation);

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

        float displacement = (4 * radius * Mathf.Sin(halfAngle)) / (3 * angleRadians);

        Vector3 centroid = centroidDirection * displacement;

        return centroid;
    }

    private float CalculateMagnitudeOfBuoyantForce(Quaternion orientation)
    {
        orientation.ToAngleAxis(out float angleDegrees, out Vector3 axis);
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return 2f / 3f * Mathf.PI * radius * radius * radius * angleRadians * weightPerUnitCubeFluid;
    }
}