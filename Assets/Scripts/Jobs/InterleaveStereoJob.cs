using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct InterleaveStereoJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> Left;
    [ReadOnly] public NativeArray<float> Right;
    [WriteOnly] public NativeArray<float> Output;

    public void Execute(int index)
    {
        if ((index & 1) == 0) // even -> left sample
            Output[index] = Left[index / 2];
        else                  // odd -> right sample
            Output[index] = Right[index / 2];
    }
}