using System.Collections.Generic;
using UnityEngine;

public sealed class KeypressEntry : Entry, IHasDummy<KeypressEntry>
{
    public readonly float TimeStamp;
    public readonly float TopDown;
    public readonly float FrontBack;
    public readonly float LeftRight;
    public readonly float ClockwiseCounterClockwise;
    public KeypressEntry(float TimeStamp, float TopDown, float FrontBack, float LeftRight, float ClockwiseCounterClockwise)
    {
        this.TimeStamp = TimeStamp;
        this.TopDown = TopDown;
        this.FrontBack = FrontBack;
        this.LeftRight = LeftRight;
        this.ClockwiseCounterClockwise = ClockwiseCounterClockwise;
    }
    public static KeypressEntry Dummy => new(0f, 0f, 0f, 0f, 0f);
}

public class KeypressLogger : Logger
{
    public readonly LogBook logBook = new(KeypressEntry.Dummy);

    private static readonly KeyCode[] posKeys = { KeyCode.R, KeyCode.W, KeyCode.A, KeyCode.Q };
    private static readonly KeyCode[] negKeys = { KeyCode.F, KeyCode.S, KeyCode.D, KeyCode.E };
    private readonly float[] durations = new float[4];

    public override IReadOnlyList<Entry> LogBook => logBook.Get;

    private void Update()
    {
        for (int i = 0; i < durations.Length; i++)
            UpdateDuration(Input.GetKey(posKeys[i]), Input.GetKey(negKeys[i]), Time.deltaTime, ref durations[i]);
        logBook.AddEntry(new KeypressEntry(
            Time.time,
            durations[0],
            durations[1],
            durations[2],
            durations[3]
        ));
    }

    private static void UpdateDuration(bool pos, bool neg, float deltat, ref float current)
        => current = !(pos ^ neg)
            ? 0 
            : pos 
                ? (current >= 0 ? current + deltat : deltat)
                : (current <= 0 ? current - deltat : -deltat);
}