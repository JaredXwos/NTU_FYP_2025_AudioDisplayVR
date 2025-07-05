using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BinauralWhiteNoiseAudioGenerator : AudioGenerator
{
    protected override int ChannelCount => 2;

    protected override int SubBufferCount => 1;

    protected override float SubBufferMinimumInterval => 0;

    protected override void BackgroundBufferRefresh()
    {
        throw new System.NotImplementedException();
    }
}
