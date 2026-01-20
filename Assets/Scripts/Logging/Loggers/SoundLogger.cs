using System.Collections.Generic;
using UnityEngine;

public sealed class BinauralEntry : Entry, IHasDummy<BinauralEntry>
{
    public readonly float Timestamp;
    public readonly float Left;
    public readonly float Right;
    public BinauralEntry(float timestamp, float left, float right)
    {
        Timestamp = timestamp;
        Left = left;
        Right = right;
    }
    public static CollidedEntry Dummy => new(0f, false);
}

public class SoundLogger : Logger
{
    [SerializeField, Range(1, 30)] private int LogDuration;
    private int deltaTime;
    private float left;
    private bool isLeft = true;
    private float timeStamp = 0f;

    public readonly LogBook logBook = new(BinauralEntry.Dummy);
    private void Awake()
        => deltaTime = 1/AudioSettings.outputSampleRate;
    public void Log(float sample)
    {
        if (isLeft) { 
            left = sample; 
            isLeft = false;
        }
        else
        {
            logBook.AddEntry(new BinauralEntry(timeStamp, left, sample));
            timeStamp += deltaTime;
            isLeft = true;
        }
    }
    public override IReadOnlyList<Entry> LogBook => logBook.Get;

}