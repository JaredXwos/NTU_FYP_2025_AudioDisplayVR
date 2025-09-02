using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

public class SequentialSingleToneFCAG : FitCueAudioGenerator
{
    protected override void BackgroundBufferRefresh()
    {
        NativeArray<JobHandle> handles = new(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        JobHandle handle = default;

        int writeable = 0;

        while(!token.IsCancellationRequested)
        {
            int[] selection = noteIndexInput.NoteIndex.Index;
            frequency = frequencyInput.Frequency;
            float[] notes = noteIndexInput.NoteIndex.Validity? validChord.Generate(frequency) : invalidChord.Generate(frequency);

            if (handle.IsCompleted)
            {
                handle.Complete();
                lastWritenBufferIndex = writeable;

                writeable = 0;
                while (writeable == readBufferIndex || writeable == lastWritenBufferIndex) writeable++;

                for (int i = 0; i < 3; i++)
                {
                    NativeArray<float> constituentarray = outputBuffers[writeable][0].GetSubArray(i * SubBufferLength * toneLength, SubBufferLength * toneLength);
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref constituentarray, AtomicSafetyHandle.Create());
                    handles[i] = new GenerateToneJob
                    {
                        frequency = notes[selection[i]],
                        sampleRate = sampleRate,
                        samples = constituentarray,
                    }.Schedule(constituentarray.Length, 64);
                }
                handle = JobHandle.CombineDependencies(handles);
            }
        }
        
        handles.Dispose();
    }
}