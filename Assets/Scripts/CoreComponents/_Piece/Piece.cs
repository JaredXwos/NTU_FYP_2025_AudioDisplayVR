using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : CollidingComponent, IGrabbable, IHas<EventHandler<FitEvent, IPParentCoreComponent>>, ILimitedAccess, IColourable
{
    [Header("Stack Configuration")]
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;
    [SerializeField] private bool heightResettable = true;

    [Header("Stack Heights")]
    [SerializeField] private Vector3Int stackHeights = Vector3Int.zero;

    [Header("Collision Information")]
    [Tooltip("Legality of target transform. Actual transform defaults to last legal transform.")]
    [SerializeField] private volatile bool illegal;
    [SerializeField] private string[] collidee;

    [SerializeField] private bool canBeMoved;

    private readonly Volatile<Vector3Int> piecePosition = new(Vector3Int.zero);
    private readonly Volatile<int> pieceOrientation = new(0);

    #region MonoBehavior
    protected override void Awake()
    {
        CanBeMoved = true;
        Auth = new(this);
        Handler = new(
            (IPParentCoreComponent payload) => { if (payload.Parent == this && heightResettable) ResetHeights(); },
            gameObject.name
        );
        piecePosition.Value = Vector3Int.RoundToInt(transform.position);
        if(stackHeights == Vector3Int.zero)
            ResetHeights();
        else ResetHeights(stackHeights);
        base.Awake();
    }
    protected virtual void Update()
    {
        canBeMoved = CanBeMoved;
        if (World.CheckCollision(targetBody, out Dictionary<Vector3Int, CoreComponent> collided))
            collidee = collided.Select(c => c.Value.name).ToHashSet().ToArray();
        illegal = !AttemptUpdate();
        transform.position = piecePosition.Value;
        Render();
    }
    #endregion

    #region IGrabbable
    public bool CanBeMoved { get; private set; }
    public int Orientation => pieceOrientation.Value;
    public Vector3 Position => piecePosition.Value;
    public void SetTransform(Vector3? position, int? orientation)
    {
        if (!CanBeMoved) return;

        Vector3Int pos = Vector3Int.RoundToInt(position ?? piecePosition.Value);
        int rot = orientation ?? pieceOrientation.Value;

        targetBody = targetBody
            .Select(t => t - piecePosition.Value)
            .Select(v =>
            (((pieceOrientation.Value % 4) + 4) % 4) switch
            {
                0 => v,
                1 => RotateY90CCW(v),
                2 => RotateY180(v),
                3 => RotateY90CW(v),
                _ => throw new System.ArgumentOutOfRangeException()
            })
            .Select(v =>
            (((rot % 4) + 4) % 4) switch
            {
                0 => v,
                1 => RotateY90CW(v),
                2 => RotateY180(v),
                3 => RotateY90CCW(v),
                _ => throw new System.ArgumentOutOfRangeException()
            })
            .Select(t => t + pos)
            .ToHashSet();

        piecePosition.Value = pos;
        pieceOrientation.Value = rot;
    }
    public void SetCanBeMoved(object Key, bool canBeMoved)
    {
        Auth.Verify(Key);
        CanBeMoved = canBeMoved;
    }
    #endregion

    #region IHasHandler
    EventHandler<FitEvent, IPParentCoreComponent> Handler;
    EventHandler<FitEvent, IPParentCoreComponent> IHas<EventHandler<FitEvent, IPParentCoreComponent>>.Handler => Handler;
    #endregion

    #region LimitedAccess
    Auth Auth;
    Auth ILimitedAccess.Auth => Auth;
    void ILimitedAccess.Authenticate() => Auth.Authenticate();
    #endregion

    #region CoreComponent
    protected override (string name, System.Func<object> binding)[] Bindings => new (string name, System.Func<object> binding)[0];
    #endregion

    public void ResetHeights(Vector3Int Heights = default)
    {
        // Use Vector3Int.zero for correct type comparison
        int[] heights = Heights == Vector3Int.zero
            ? Enumerable.Range(0, 3)
                .Select(_ => Random.Range(minGeneratedHeight, maxGeneratedHeight + 1))
                .ToArray()
            : new int[] { Heights.x, Heights.y, Heights.z };

        stackHeights = new(heights[0], heights[1], heights[2]);

        targetBody.Clear();
        for (int index = 0; index < 3; index++)
        {
            int x = index - 1;
            int h = heights[index];
            for (int y = 0; y > -h; y--)
                targetBody.Add(new Vector3Int(x, y, 0) + Vector3Int.RoundToInt(piecePosition.Value));
        }

        stackHeights = new Vector3Int(heights[0], heights[1], heights[2]);
    }
    public void SetMaterialColor(Color color, object key)
    {
        Auth.Verify(key);
        if (Material != null)
            Material.color = color;
    }

    public Color GetMaterialColor() => (Material??sharedMaterial).color;
    
}