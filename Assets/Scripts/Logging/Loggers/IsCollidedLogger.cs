using System.Collections.Generic;
using UnityEngine;
public interface ICollidedState
{
    bool IsCollided { get; }
}

public sealed class CollidedEntry : Entry, IHasDummy<CollidedEntry>
{
    public readonly float Timestamp;
    public readonly bool IsCollided;
    public CollidedEntry(float timestamp, bool isCollided)
    {
        this.Timestamp = timestamp;
        this.IsCollided = isCollided;
    }
    public static CollidedEntry Dummy => new(0f, false);
}

public class IsCollidedLogger : Logger
{
    public readonly LogBook logBook = new(CollidedEntry.Dummy);
    public override IReadOnlyList<Entry> LogBook => logBook.Get;
    private ICollidedState CollidedState;
    private void Awake() => Check.PropertyEnabledElseAssign<ICollidedState>(this, "CollidedState");
    private void Update() => logBook.AddEntry(new CollidedEntry(Time.time, CollidedState.IsCollided));
}