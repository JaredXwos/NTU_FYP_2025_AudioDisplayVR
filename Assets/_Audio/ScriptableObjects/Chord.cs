using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChord", menuName = "Audio/Chord")]
public class Chord : ScriptableObject
{
    public float[] ratios;
    public float[] Generate(float frequency) => ratios.Select(i => i * frequency).ToArray();
}