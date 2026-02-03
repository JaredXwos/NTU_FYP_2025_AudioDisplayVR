using System.Collections.Generic;
using UnityEngine;

public sealed class BinauralEntry : Entry, IHasDummy<BinauralEntry>
{
    public readonly float Left;
    public readonly float Right;
    public BinauralEntry(float left, float right)
    {
        Left = left;
        Right = right;
    }
    public static BinauralEntry Dummy => new(0, 0);
}

public class SoundLogger : Logger
{
    private float left;
    private bool isLeft = true;

    public readonly LogBook logBook = new(BinauralEntry.Dummy);
    public void Log(float sample)
    {
        if (isLeft) { 
            left = sample; 
            isLeft = false;
        }
        else
        {
            logBook.AddEntry(new BinauralEntry(left, sample));
            isLeft = true;
        }
    }
    public override IReadOnlyList<Entry> LogBook => logBook.Get;

}