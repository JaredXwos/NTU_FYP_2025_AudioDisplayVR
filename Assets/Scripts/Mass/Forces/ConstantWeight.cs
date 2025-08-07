using UnityEngine;

public class ConstantWeight : MonoBehaviour, ILoad, ILimitedAccess
{
    [SerializeField] private float Weight = 0f;

    private readonly Volatile<float> weight = new();
    private readonly Volatile<Vector3> centreOfGravity = new();

    private void Awake()
    {
        Auth = new(this);
        centreOfGravity.Value = transform.position;
        weight.Value = Weight;
    }

    protected void Update()
    {
        centreOfGravity.Value = transform.position;
        Weight = weight.Value;
    }

    #region ILoad
    public Vector3 Force => Vector3.down * weight.Value;
    public Vector3 Position => centreOfGravity.Value;
    #endregion

    #region ILimitedAccess
    Auth Auth;
    Auth ILimitedAccess.Auth => Auth;
    void ILimitedAccess.Authenticate() => Auth.Authenticate();
    #endregion

    public void SetWeight(float Weight, object key)
    {
        Auth.Verify(key);
        weight.Value = Weight;
    }
}