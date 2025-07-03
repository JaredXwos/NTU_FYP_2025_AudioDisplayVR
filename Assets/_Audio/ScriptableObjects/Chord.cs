using UnityEngine;

[CreateAssetMenu(fileName = "NewChord", menuName = "Audio/Chord")]
public class Chord : ScriptableObject
{
    [SerializeField] public float[] ratios;
    public float[] Generate(float frequency) =>
        System.Array.ConvertAll(ratios, r => frequency * r);
}