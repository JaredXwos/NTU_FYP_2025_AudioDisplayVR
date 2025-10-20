using Unity.Mathematics;
using UnityEngine;

public class ConstantAngleDistanceInputProvider : MonoBehaviour, IHasAngle, IHasDistance
{
    [SerializeField, Range(-180f, 180f)] private float angleDegrees = 0f;
    [SerializeField, Range(0f, 10f)] private float distance = 1f;

    private AngleDistanceChirpGeneratorInput Output;
    private void Awake() => Check.PropertyEnabledElseAssign<AngleDistanceChirpGeneratorInput>(this, "Output");
    private void Update()
    {
        if(Output != null)
        {
            ChirpGeneratorInput input = Output.ChirpInput;
            Debug.Log($"Angle: {math.degrees(Angle):F1}, Distance: {Distance:F2}m, Duration: {input.Duration} frames, Freq: {input.StartFreq:F1}Hz to {input.EndFreq:F1}Hz, Source: {input.Source}");
        }
    }
    public float Angle => math.radians(angleDegrees);
    public float Distance => distance;

    
}