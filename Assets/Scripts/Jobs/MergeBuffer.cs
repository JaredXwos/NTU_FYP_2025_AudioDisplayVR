using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct MergeBufferJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> source;
    public NativeArray<float> destination;

    public void Execute(int i)
    {
        destination[i] += source[i];
    }
}
