using UnityEngine;

public class ScaleBalanceAngularDisplacementBWNAGII : MonoBehaviour, IBinauralWhiteNoiseAGII
{
    [SerializeField] private IHasOrientation scaleBalance;

    private void Awake()
    {
        if(!Check.PropertyEnabledElseAssign<IHasOrientation>(this, "scaleBalance")) return;
    }

    public Vector2 RelativeSourcePosition
    {
        get
        {
            Vector3 q = scaleBalance.Orientation;
            float axisComponentMagnitude = new Vector3(q.x, q.y, q.z).magnitude * Mathf.Sign(-q.z);
            return new Vector2(axisComponentMagnitude, 0);
        }
    }
}