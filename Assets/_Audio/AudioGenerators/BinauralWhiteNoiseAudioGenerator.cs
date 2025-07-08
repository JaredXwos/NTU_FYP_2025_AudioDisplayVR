using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(AudioSource))]
public class BinauralWhiteNoiseAudioGenerator : AudioGenerator
{

    protected override int ChannelCount => 2;

    protected override int SubBufferCount => 1;

    protected override float SubBufferMinimumInterval => 0;

    protected override void BackgroundBufferRefresh()
    {
        NativeArray<float> whiteBuffer = new(outputBuffers[0][0].Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        NativeArray<Unity.Mathematics.Random> randoms = new(outputBuffers[0][0].Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        Unity.Mathematics.Random seeder = new(1);
        for (int i = 0; i < randoms.Length; i++)
            randoms[i] = new Unity.Mathematics.Random(seeder.NextUInt() + 1);

        while (!token.IsCancellationRequested)
        {
            
        }
    }
}
