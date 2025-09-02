using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class SpatialiseMonophonic : BufferProcessor<float>
{
    private readonly IBinauralWhiteNoiseAGII inputInterface;
    private NativeReference<SpatializationParams> sparams = new(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    private readonly float2 earDistance;
    private readonly float tiltSensitivity;
    private readonly float centreAttenuationFactor ;

    public SpatialiseMonophonic(
        NativeArray<float>[] input,
        IBinauralWhiteNoiseAGII inputInterface,
        float2 earDistance,
        float tiltSensitivity,
        float centreAttenuationFactor

    ) : base(input)
    {
        this.inputInterface = inputInterface ?? throw new System.ArgumentNullException(nameof(inputInterface), "Input interface cannot be null");
        this.earDistance = earDistance;
        this.tiltSensitivity = tiltSensitivity;
        this.centreAttenuationFactor = centreAttenuationFactor;
        Initialise();
    }

    public override void Dispose()
    {
        base.Dispose();
        if ( sparams.IsCreated )  sparams.Dispose();
    }

    protected override (int inputArrayCount, int outputArrayCount) ArrayCount => (1, 2);

    protected override void InternalProcess()
    {
        sparams.Value = SpatializationParams.Create(
            earDistance,
            inputInterface.RelativeSourcePosition * tiltSensitivity,
            343.0f,
            samplerate,
            centreAttenuationFactor
        );

        new SpatializeAddJob
        {
            input = input[0],
            Params = sparams,
            outputLeft = output[0],
            outputRight = output[1],
        }.Schedule(input[0].Length, 64).Complete();
    }
}