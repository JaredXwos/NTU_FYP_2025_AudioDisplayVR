using UnityEngine;

public record WeakeningWeightReachesZeroPayload(
    CoreComponent Parent,
    WeakeningWeight Caller
) : EventPayload, IPParentCoreComponent, IPCaller<WeakeningWeight>;

public class WeakeningWeight : Dispatch, ILoad, IWeaken, IRefresh, ILimitedAccess
{
    [SerializeField] private float Weight = 0f;
    [SerializeField] private float MaxWeight = 100f;
    [SerializeField] private float MinWeight = 10f;
    [SerializeField] private int TotalStage = 0;
    [SerializeField] private float CurrentStage = 0;

    private readonly Volatile<float> weight = new();
    private readonly Volatile<Vector3> centreOfGravity = new();

    #region MonoBehavior
    protected override void Awake()
    {
        Auth = new(this);
        EventType = typeof(ReachesZeroEvent);
        PayloadType = typeof(WeakeningWeightReachesZeroPayload);
        base.Awake();
        Refresh();
        centreOfGravity.Value = transform.position;
    }

    private void Update() => centreOfGravity.Value = transform.position;
    #endregion

    #region ILoad
    public Vector3 Force => Vector3.down * weight.Value;
    public Vector3 Position => centreOfGravity.Value;
    #endregion

    #region IWeaken
    public void Weaken()
    {
        if(!enabled) return;
        CurrentStage++;
        weight.Value = (TotalStage - CurrentStage) * Weight;
        if (CurrentStage == TotalStage) 
            Invoke(new WeakeningWeightReachesZeroPayload(GetComponent<CoreComponent>(), this));
        
    }
    #endregion

    #region IRefresh
    public void Refresh()
    {
        if(!enabled) return;
        Weight = UnityEngine.Random.Range(MinWeight, MaxWeight);
        weight.Value = Weight;
    }
    #endregion

    #region ILimitedAccess
    Auth Auth;
    Auth ILimitedAccess.Auth => Auth;
    void ILimitedAccess.Authenticate() => Auth.Authenticate();
    #endregion
}