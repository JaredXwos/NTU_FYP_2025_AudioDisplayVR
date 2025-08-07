using System.Linq;
using Unity.Collections;
using UnityEngine;

public class GroundClearanceWeightFCAGII : MonoBehaviour, INoteIndexInput, IFrequencyInput
{
    [SerializeField] private Sonar Sonar;
    private ILoad Weight;

    [SerializeField, ReadOnly] private NoteIndex _noteIndex = new();
    [SerializeField, ReadOnly] private float _frequency;

    [SerializeField] private float minimumFrequency;
    [SerializeField] private float maximumFrequency;
    [SerializeField] private float maximumWeight;
    [SerializeField] private float exponent;

    private readonly Volatile<NoteIndex> noteIndex = new(new NoteIndex(false, null));
    private readonly Volatile<float> frequency = new();

    private void Awake()
    {
        if (
            !Check.PropertyEnabledElseAssign<Sonar>(this, "Sonar") ||
            !Check.PropertyEnabledElseAssign<ILoad>(this, "Weight")
        )
        {
            Debug.LogWarning("[Ground Clearance Weight Fit Cue Audio Generator Input Interface] Not all components found, disabling");
            enabled = false;
            return;
        }

    }

    private void Update()
    {
        UpdateNoteIndex();
        UpdateFrequency();
    }

    private void UpdateNoteIndex()
    {
        int[] selection;
        (selection, _noteIndex.Validity) = Sonar.GetClearance();
        _noteIndex.Index = selection.Distinct().Count() > 2 ?
            selection                                   // if there are three distinct values,
            .Select(v => selection.Count(x => x < v))   // assign 0, 1, 2 largest to smallest
            .ToArray() : 
            selection                                   // else (1 or 2 distinct values
            .Select(v => selection.Max() - v)           // each value is the distance from max,
            .Select(v => v > 2? 2 : v)                  // clipped at 2
            .ToArray();
        noteIndex.Value = _noteIndex;
    }

    private void UpdateFrequency()
    {
        if (maximumWeight <= 0f || minimumFrequency <= 0f || maximumFrequency <= minimumFrequency) return;
        float t = 1f - Mathf.Pow(Mathf.Clamp01(Weight.Force.magnitude / maximumWeight), exponent);
        _frequency =  Mathf.Exp(Mathf.Lerp(Mathf.Log(minimumFrequency), Mathf.Log(maximumFrequency), t));
        frequency.Value = _frequency;
    }

    public NoteIndex NoteIndex => noteIndex.Value;

    public float Frequency => frequency.Value;
}