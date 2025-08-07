using System.Linq;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(CollidingComponent))]
public class ScaleBalance : AbstractScaleBalance, IHasOrientation
{
    [SerializeField] private float momentOfInertia = 2f;
    [SerializeField, ReadOnly] private Vector3 angularAcceleration = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularVelocity = Vector3.zero;
    [SerializeField, ReadOnly] private Vector3 angularDisplacement = Vector3.zero;

    private readonly Volatile<Vector3> _angularDisplacement = new(Vector3.zero);

    #region MonoBehavior
    private void FixedUpdate()
    {
        weightSet.RemoveWhere(load => (Object)load == null);
        torques = weightSet
            .Select(weight => new GameObjectInt { gameObject = weight.gameObject, value = (int)Vector3.Cross(weight.Position - origin, weight.Force).magnitude })
            .ToArray();
        angularDisplacement += 0.5f * Time.fixedDeltaTime * Time.fixedDeltaTime * angularAcceleration + angularVelocity * Time.fixedDeltaTime;

        angularVelocity += 0.1f * Time.fixedDeltaTime * angularAcceleration;
        angularAcceleration = weightSet
            .Select(weight =>
            {
                Vector3 r = weight.Position - origin;
                Vector3 F = weight.Force;

                Vector3 torque = Vector3.Cross(r, F);

                return torque;
            })
            .Aggregate(Vector3.zero, (sum, torque) => sum + torque);

        angularAcceleration /= momentOfInertia;

        _angularDisplacement.Value = angularDisplacement;
    }
    #endregion

    public Vector3 Orientation => _angularDisplacement.Value;
    public Quaternion GetOrientation => Quaternion.AngleAxis(
        _angularDisplacement.Value.magnitude * Mathf.Rad2Deg,
        _angularDisplacement.Value.normalized);
}