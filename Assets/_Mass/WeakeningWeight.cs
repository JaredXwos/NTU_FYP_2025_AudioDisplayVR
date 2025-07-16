using UnityEngine;

public class WeakeningWeight : MonoBehaviour, ILoad, IWeaken, IRefresh
{
    [SerializeField] private float Weight = 0f;
    [SerializeField] private float MaxWeight = 100f;
    [SerializeField] private float MinWeight = 10f;
    [SerializeField] private int TotalStage = 0;
    [SerializeField] private float CurrentStage = 0;

    private readonly Volatile<float> weight = new();
    private readonly Volatile<Vector3> centreOfGravity = new();

    private void Awake()
    {
        Refresh();
        centreOfGravity.Value = transform.position;
    }

    private void Update() => centreOfGravity.Value = transform.position;

    public Vector3 Force => Vector3.down * weight.Value;
    public Vector3 Position => centreOfGravity.Value;

    public void Weaken()
    {
        CurrentStage++;
        weight.Value = (TotalStage - CurrentStage) * Weight;
        if (CurrentStage == TotalStage)
        {
            if (TryGetComponent<Death>(out var death)) death.Trigger();
            enabled = false;
        }
    }

    public void Refresh()
    {
        Weight = Random.Range(MinWeight, MaxWeight);
        weight.Value = Weight;
    }
}