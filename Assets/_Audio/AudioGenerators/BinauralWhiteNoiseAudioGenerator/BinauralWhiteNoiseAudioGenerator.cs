using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows;

public interface IBinauralWhiteNoiseAGII
{
    public Vector2 RelativeSourcePosition { get; }
}

[RequireComponent(typeof(AudioSource))]
public class BinauralWhiteNoiseAudioGenerator : AudioGenerator
{
    [SerializeField] private MonoBehaviour InputInterface;
    [SerializeField] private float2 earDistance = new(0.5f, 0);
    [SerializeField] private int randomBufferCount = 1; 
    [SerializeField] private float tiltSensitivity = 1f;
    [SerializeField] private float centreAttenuationFactor = 0f;
    [SerializeField, ReadOnly] private Vector2 sourcePosition;

    private IBinauralWhiteNoiseAGII inputInterface;


    protected override void Awake()
    {
        base.Awake();
        if (InputInterface != null && InputInterface.enabled && InputInterface is IBinauralWhiteNoiseAGII b) inputInterface = b;
        else
        {
            Check.PropertyEnabledElseAssign<IBinauralWhiteNoiseAGII>(this, "inputInterface");
            if (inputInterface is MonoBehaviour m) InputInterface = m;
        }
        if(randomBufferCount < 1)
        {
            Debug.LogWarning("[White Noise Generator] Random Buffer Count less than 1. Setting to 1");
            randomBufferCount = 1;
        }
    }

    protected override int ChannelCount => 2;

    protected override int SubBufferCount => 1;

    protected override float SubBufferMinimumInterval => 0;

    protected override void BackgroundBufferRefresh()
    {
        NativeArray<float> whiteBuffer = new(outputBuffers[0][0].Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        NativeArray<Unity.Mathematics.Random>[] randomRings = new NativeArray<Unity.Mathematics.Random>[randomBufferCount];
        NativeReference<SpatializationParams> sparams = new(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        JobHandle job = default;
        int writeable = 0;
        int currentRandomBuffer = 0;

        Unity.Mathematics.Random seeder = new(1);
        for (int i = 0; i < randomRings.Length; i++)
        {
            randomRings[i] = new(outputBuffers[0][0].Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int j = 0; j < randomRings[i].Length; j++)
                randomRings[i][j] = new Unity.Mathematics.Random(seeder.NextUInt() + 1);
        }

        while (!token.IsCancellationRequested)
        {
            if (job.IsCompleted)
            {
                job.Complete();
                for (int i = 0; i < outputBuffers[writeable].Length; i++)
                    for (int j = 0; j < outputBuffers[writeable][i].Length; j++)
                        outputBuffers[writeable][i][j] = whiteBuffer[j];

                lastWritenBufferIndex = writeable;
                writeable = 0;
                while (writeable == readBufferIndex || writeable == lastWritenBufferIndex) writeable++;

                job = new GenerateRandomToneJob
                {
                    maxAmplitude = 0.5f,
                    randoms = randomRings[currentRandomBuffer++],
                    samples = whiteBuffer
                }.Schedule(whiteBuffer.Length, 64);
                if (currentRandomBuffer >= randomRings.Length) currentRandomBuffer = 0;
            }
            sourcePosition = inputInterface.RelativeSourcePosition * tiltSensitivity;
        }

        whiteBuffer.Dispose();
        foreach(NativeArray<Unity.Mathematics.Random> buffer in randomRings) buffer.Dispose();
    }
}
