using System.Linq;
using UnityEngine;

public class StaticScaleBalance : AbstractScaleBalance
{
    [SerializeField] private Vector3 Torque;
    private Volatile<Vector3> netTorque = new();
    protected override void Update()
    {
        weightSet.RemoveWhere(load => (Object) load == null);
        torques = weightSet
            .Select(weight => new GameObjectInt { 
                gameObject = weight.gameObject, 
                value = (int) Vector3.Cross(weight.Position - origin, weight.Force).magnitude
            })
            .ToArray();

        netTorque.Value = weightSet
            .Select(weight =>
            {
                Vector3 r = weight.Position - origin;
                Vector3 F = weight.Force;
                Vector3 torque = Vector3.Cross(r, F);
                return torque;
            })
            .Aggregate(Vector3.zero, (sum, torque) => sum + torque);
        Torque = netTorque.Value;
    }
    public Vector3 NetTorque => netTorque.Value;
}