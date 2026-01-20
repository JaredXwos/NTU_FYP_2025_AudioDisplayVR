using Unity.Mathematics;
using UnityEngine;

public class ConstantChirpInputProvider : MonoBehaviour, IChirpGeneratorInput
{
    [SerializeField, Range(100, 200000)] private int Duration;
    [SerializeField, Range(80, 1500)] private float StartFreq;
    [SerializeField, Range(80, 1500)] private float EndFreq;
    [SerializeField] private Vector2 Source;
    [SerializeField] private int DurationPerSecond;

    private void Awake() => DurationPerSecond = AudioSettings.outputSampleRate;
    public ChirpGeneratorInput NextChirpInput() => new(
        duration: Duration,        // e.g. 0.1 sec at 48 kHz
        startFreq: StartFreq,       // A4
        endFreq: EndFreq,         // A5
        source: new float2(Source) // source position in 2D space
    );
}