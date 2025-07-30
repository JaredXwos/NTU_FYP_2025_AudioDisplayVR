using System;
using UnityEngine;

[Serializable]
public struct NoteIndex
{
    public int[] Index;
    public bool Validity;

    public NoteIndex(bool validity, int[] index)
    {
        Validity = validity;
        Index = index ?? new int[3];
    }
}

public interface INoteIndexInput
{
    NoteIndex NoteIndex {  get; }
}

public interface IFrequencyInput
{
    float Frequency { get; }
}

public abstract class FitCueAudioGenerator : AudioGenerator
{
    [Header("Components")]
    // -----------------------------------------------------------------------------

    [SerializeField] protected INoteIndexInput noteIndexInput;
    [SerializeField] protected IFrequencyInput frequencyInput;

    [Tooltip("Set of notes to be played when the input is valid")]
    [SerializeField] protected Chord validChord;

    [Tooltip("Set of notes to be played when the input is not valid")]
    [SerializeField] protected Chord invalidChord;

    [Header("Signal")]
    // -----------------------------------------------------------------------------

    [Tooltip("Length of each individual signal of the 3 signals, in units")]
    [SerializeField] protected int toneLength = 1;

    [Tooltip("Length of the post-signal silence, in units")]
    [SerializeField] private int silenceLength = 3;

    [Tooltip("Length of the units used for the tone and silence length, in miliseconds")]
    [SerializeField] private float unitInterval = 100;

    [SerializeField] protected float frequency = 261.626f; // C4

    #region MonoBehavior
    protected override void Awake()
    {
        if (!Check.PropertyEnabledElseAssign<INoteIndexInput>(this, "noteIndexInput"))
        {
            Debug.LogWarning($"[{GetType().Name}] No valid note index input assigned on {gameObject.name}, disabling component.");
            enabled = false;
            return;
        }

        if (!Check.PropertyEnabledElseAssign<IFrequencyInput>(this, "frequencyInput"))
        {
            Debug.LogWarning($"[{GetType().Name}] No valid note index input assigned on {gameObject.name}, disabling component.");
            enabled = false;
            return;
        }

        if (validChord == null)
        {
            Debug.LogWarning($"[{GetType().Name}] No validChord assigned on {gameObject.name}, using silence.");
            validChord = ScriptableObject.CreateInstance<Chord>();
            validChord.ratios = new float[] { 0f, 0f, 0f };
        }
        if (invalidChord == null)
        {
            Debug.LogWarning($"[{GetType().Name}] No invalidChord assigned on {gameObject.name}, using silence.");
            invalidChord = ScriptableObject.CreateInstance<Chord>();
            invalidChord.ratios = new float[] { 0f, 0f, 0f };
        }
        base.Awake();
    }
    #endregion

    #region AudioGenerator
    protected override int ChannelCount => 1;
    protected override int SubBufferCount => toneLength * 3 + silenceLength;
    protected override float SubBufferMinimumInterval => unitInterval;
    #endregion AudioGenerator
}
