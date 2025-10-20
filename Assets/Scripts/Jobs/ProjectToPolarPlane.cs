using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
struct ProjectToPlaneAndPolarJob : IJobParallelFor
{
    [ReadOnly] public float3 origin;
    [ReadOnly] public float3 u; // plane X
    [ReadOnly] public float3 v; // plane Y

    [ReadOnly] public NativeArray<float3> worldPts;

    [WriteOnly] public NativeArray<PolarPoint> polarPts;

    public void Execute(int i)
    {
        float3 toPt = worldPts[i] - origin;
        float x = math.dot(toPt, u);
        float y = math.dot(toPt, v);

        float r = math.length(new float2(x, y));
        float theta = math.atan2(y, x);

        polarPts[i] = new PolarPoint(r, theta);
    }
}