using Unity.Collections;
using UnityEngine;

public class WeakeningWeight : MonoBehaviour, ILoad, IWeaken
{
    [SerializeField] private float Weight = 0f;
    [SerializeField] private int TotalStage = 0;
    [SerializeField, ReadOnly] private float CurrentStage = 0;

    private readonly Volatile<float> weight = new();
    private readonly Volatile<Vector3> centreOfGravity = new();

    private void Awake()
    {
        weight.Value = Weight;
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
}