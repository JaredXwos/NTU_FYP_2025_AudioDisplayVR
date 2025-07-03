using System.Linq;
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

            int[] selected = input.GetToneInput();
            float[] notes = selected.Select(x => x < 0).Any() ? invalidChord.Generate(frequency) : validChord.Generate(frequency);
            selected = selected.Distinct().Count() > 2 ?
                selected.Select(v => selected.Count(x => x < v)).ToArray() : // if there are three distinct values, assign 0, 1, 2 largest to smallest
                selected.Select(v =>                                         // if there are less than 2 distinct values
                    v == selected.Max() ? 0 :                                // assign all largest values 0
                    v > selected.Max() - 1 ? 2 :                             // else if the value deviates from the largest by more than 2 or more, clip it to 2
                    v
                ).ToArray();

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
                        frequency = notes[selected[i]],
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