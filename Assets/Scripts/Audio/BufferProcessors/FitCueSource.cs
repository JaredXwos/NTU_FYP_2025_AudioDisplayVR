using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public sealed class FitCueSource : BufferProcessor<float>
{

    [Tooltip("Set of notes to be played when the input is valid")]
    private readonly Chord validChord;

    [Tooltip("Set of notes to be played when the input is not valid")]
    private readonly Chord invalidChord;

    private readonly INoteIndexInput noteIndexInput;
    private readonly IFrequencyInput frequencyInput;
    private readonly int toneLength;
    private NativeArray<JobHandle> handles = new(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

    public FitCueSource(
        NativeArray<float>[] input,
        INoteIndexInput noteIndexInput,
        IFrequencyInput frequencyInput,
        Chord validChord,
        Chord invalidChord,
        int toneLength
    ) : base(input)
    {
        if (toneLength * 3 > output[0].Length)
            throw new ArgumentException("Signal length must be less than a third of the input buffer length.");
        
        this.noteIndexInput = noteIndexInput ?? throw new ArgumentNullException(nameof(noteIndexInput), "Note index input cannot be null");
        this.frequencyInput = frequencyInput ?? throw new ArgumentNullException(nameof(frequencyInput), "Frequency input cannot be null");
        this.toneLength = toneLength;
        this.validChord = validChord ?? throw new ArgumentNullException(nameof(validChord), "Valid chord cannot be null");
        this.invalidChord = invalidChord ?? throw new ArgumentNullException(nameof(invalidChord), "Invalid chord cannot be null");
        Initialise();
    }

    protected override (int inputArrayCount, int outputArrayCount) ArrayCount => (1, 1);

    protected override void InternalProcess()
    {
        int[] selection = noteIndexInput.NoteIndex.Index;
        float[] notes = noteIndexInput.NoteIndex.Validity ? 
            validChord.Generate(frequencyInput.Frequency) : 
            invalidChord.Generate(frequencyInput.Frequency);
        for (int i = 0; i < 3; i++)
        {
            NativeArray<float> constituentarray = output[0].GetSubArray(i * toneLength, toneLength);
            handles[i] = new GenerateToneUnsafeJob
            {
                frequency = notes[selection[i]],
                sampleRate = samplerate,
                samples = constituentarray,
            }.Schedule(constituentarray.Length, 64);
        }
        JobHandle.CombineDependencies(handles).Complete();
    }
}