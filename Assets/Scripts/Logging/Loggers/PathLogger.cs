using System.Collections.Generic;
using UnityEngine;

public sealed class PathEntry : Entry, IHasDummy<PathEntry>
{
    public readonly float Timestamp, x, y, z;
    public PathEntry(float timestamp, float x, float y, float z)
    {
        this.Timestamp = timestamp;
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static PathEntry Dummy => new(0f, 0f, 0f, 0f);
}

public class PathLogger : Logger
{
    public readonly LogBook logBook = new(PathEntry.Dummy);

    [SerializeField] private MonoBehaviour PathObject = null;

    public override IReadOnlyList<Entry> LogBook => logBook.Get;
    private void Update()
    {
        Vector3 pos = PathObject.transform.root.position;
        logBook.AddEntry(new PathEntry(Time.time, pos.x, pos.y, pos.z));
    }
}