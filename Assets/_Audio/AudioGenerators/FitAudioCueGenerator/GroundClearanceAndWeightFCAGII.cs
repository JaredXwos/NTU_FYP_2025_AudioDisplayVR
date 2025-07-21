using System.Linq;
using Unity.Collections;
using UnityEngine;

public class GroundClearanceWeightFCAGII : MonoBehaviour, INoteIndexInput, IFrequencyInput
{
    [SerializeField] private GroundSonar GroundSonar;
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
            !Check.PropertyEnabledElseAssign<GroundSonar>(this, "GroundSonar") ||
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
        int[] selection = GroundSonar.GetGroundClearance();
        _noteIndex.Validity = !selection.Any(x => x < 0);
        _noteIndex.Index = selection.Distinct().Count() > 2 ?
            selection.Select(v => selection.Count(x => x < v)).ToArray() : // if there are three distinct values, assign 0, 1, 2 largest to smallest
            selection.Select(v =>                                          // if there are less than 2 distinct values
                v == selection.Max() ? 0 :                                 // assign all largest values 0
                v > selection.Max() - 1 ? 2 :                              // else if the value deviates from the largest by more than 2 or more, clip it to 2
                v - selection.Min()
            ).ToArray();
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