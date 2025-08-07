using UnityEngine;

[RequireComponent(typeof(StaticScaleBalance))]
public class ScaleBalanceTorqueBWNAGII : MonoBehaviour, IBinauralWhiteNoiseAGII
{
    [SerializeField] private StaticScaleBalance ScaleBalance;
    [SerializeField] private Vector2 relativeSourcePosition;
    private readonly Volatile<Vector2> relativeSourcePositionVolatile = new(Vector2.zero);
    private void Awake()
    {
        Check.PropertyEnabledElseAssign<StaticScaleBalance>(this, "ScaleBalance");
    }
    private void Update()
    {
        Vector3 q = ScaleBalance.NetTorque;
        float axisComponentMagnitude = new Vector3(q.x, q.y, q.z).magnitude * Mathf.Sign(-q.z);
        relativeSourcePositionVolatile.Value = new Vector2(axisComponentMagnitude, 0);
        relativeSourcePosition = relativeSourcePositionVolatile.Value;
    }
    public Vector2 RelativeSourcePosition => relativeSourcePositionVolatile.Value;
}