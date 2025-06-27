using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SpatializeAddJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> input;
    [ReadOnly] public int shiftL;
    [ReadOnly] public int shiftR;
    [ReadOnly] public float gainL;
    [ReadOnly] public float gainR;
    [ReadOnly] public float centerGain;

    public NativeArray<float> outputLeft;
    public NativeArray<float> outputRight;

    public void Execute(int index)
    {
        outputLeft[index] = (shiftL > index) ? 0 : input[index - shiftL] * gainL * centerGain;
        outputRight[index] = (shiftR > index) ? 0 : input[index - shiftR] * gainR * centerGain;
    }
}