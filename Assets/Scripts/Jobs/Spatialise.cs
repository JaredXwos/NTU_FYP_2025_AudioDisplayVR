using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
[BurstCompile]
public struct SpatializeAddJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> input;
    [ReadOnly] public NativeReference<SpatializationParams> Params;

    public NativeArray<float> outputLeft;
    public NativeArray<float> outputRight;

    public void Execute(int index)
    {
        outputLeft[index] += (Params.Value.shiftL > index) ? 0 : input[index - Params.Value.shiftL] * Params.Value.gainL * Params.Value.centerGain * 0.5f;
        outputRight[index] += (Params.Value.shiftR > index) ? 0 : input[index - Params.Value.shiftR] * Params.Value.gainR * Params.Value.centerGain * 0.5f;
    }
}