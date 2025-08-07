using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public record FitEventPayload : EventPayload, IPParentCoreComponent, IPActive, IPCollidees
{
    public CoreComponent Parent { get; }
    public bool IsActive { get; }
    public GameObject[] Collidees { get; }
    public FitEventPayload(CoreComponent parent, GameObject[] collidees, bool isActive)
    {
        Parent = parent;
        IsActive = isActive;
        Collidees = collidees ?? Array.Empty<GameObject>();
    }
}

class PositionComparer : IComparer<(Vector3Int position, int quotient)>
{
    public int Compare((Vector3Int position, int quotient) a, (Vector3Int position, int quotient) b)
    {
        int cmp = a.position.x.CompareTo(b.position.x);
        if (cmp != 0) return cmp;

        cmp = a.position.z.CompareTo(b.position.z);
        if (cmp != 0) return cmp;

        return a.position.y.CompareTo(b.position.y);
    }
}

[RequireComponent(typeof(CollidingComponent))]
public class Sonar : Dispatch
{
    [Header("Sonar Settings")]
    [SerializeField] protected Vector3Int direction = Vector3Int.down;
    [SerializeField] protected int maxDistance = 10;
    [SerializeField] protected bool broadcastFitEvent = true;

    [Header("Display")]
    [SerializeField] protected int[] Clearance = new int[3] { -1, -1, -1 };
    [SerializeField] protected volatile bool Valid = true;
    [SerializeField] protected bool isCurrentlyFit = false;

    protected readonly Volatile<int[]> clearance = new(new int[3]);
    protected readonly Volatile<GameObject[]> collidedObjects = new(new GameObject[0]);

    public CollidingComponent Parent { get; protected set; }

    protected IEnumerable<Transform> ComponentTransforms;

    #region MonoBehavior
    protected override void Awake()
    {
        Check.PropertyEnabledElseAssign<CollidingComponent>(this, "Parent");
        EventType = typeof(FitEvent);
        PayloadType = typeof(FitEventPayload);
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Ping();
    }
    protected virtual void Update()
    {
        Ping();
        Clearance = clearance.Value;

        if (broadcastFitEvent && Valid && Clearance.All(h => h == 0) ^ isCurrentlyFit)
            Invoke(new FitEventPayload(Parent, collidedObjects.Value, !isCurrentlyFit));
        
        isCurrentlyFit = Valid && Clearance.All(h => h == 0);
    }
    #endregion

    public (int[] clearance, bool valid) GetClearance() => (clearance.Value, Valid);
    

    protected virtual void Ping()
    {
        var boundary = new SortedSet<(Vector3Int position, int quotient)>(new PositionComparer());

        foreach ((Vector3Int position, int quotient) item in
            Parent
            .GetBody()
            .Select(v => (v, div: ScalarDivision(v, direction)))
            .GroupBy(x => x.div.remainder)
            .Select(g => g.OrderByDescending(x => x.div.quotient).First())
            .Select(g => (g.v, g.div.quotient)))
            boundary.Add(item);

        List<int> distances = new();
        HashSet<GameObject> collideds = new();
        int valid = 0;
        foreach (var(pos, quotient) in boundary)
            for(int i = 0; i < maxDistance; i++)
                if(World.CheckCollision(pos + (i+1) * direction, out CoreComponent collided))
                {
                    distances.Add(i);
                    collideds.Add(collided.gameObject);
                    valid++;
                    break;
                }

        clearance.Value = valid switch
        {
            0 => boundary.Select(p => -p.quotient).ToArray(),
            var n when n == boundary.Count => distances.ToArray(),
            _ => Enumerable.Repeat(-1, boundary.Count).ToArray()
        };
        Valid = valid == boundary.Count;
        collidedObjects.Value = collideds.ToArray();
    }

    public static (int quotient, Vector3Int remainder) ScalarDivision(Vector3Int dividend, Vector3Int divisor)
    {
        if (divisor == Vector3Int.zero) throw new DivideByZeroException("Division by zero in Scalar Division");

        int quotient = new int[]
        {
            divisor.x == 0? int.MaxValue : dividend.x / divisor.x,
            divisor.y == 0? int.MaxValue : dividend.y / divisor.y,
            divisor.z == 0? int.MaxValue : dividend.z / divisor.z
        }.OrderBy(q => Mathf.Abs(q))
        .First();

        Vector3Int remainder = new(
            dividend.x - quotient * divisor.x,
            dividend.y - quotient * divisor.y,
            dividend.z - quotient * divisor.z
        );

        return (quotient, remainder);
    }
}