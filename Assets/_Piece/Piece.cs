using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

public interface IRequirePieceInfo { }

public class Piece : MonoBehaviour, IPieceCollidable, IGrabbable, IHas<PieceFitEventHandler>
{
    [Header("Stack Configuration")]
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;
    [SerializeField] private bool heightResettable = true;

    [Header("Stack Heights")]
    [SerializeField] private Vector3Int stackHeights = Vector3Int.zero;

    [Header("Collision Information")]
    [SerializeField] private bool pieceCollisionEnabled = true;
    [Tooltip("Legality of target transform. Actual transform defaults to last legal transform.")]
    [SerializeField] private volatile bool illegal;
    [SerializeField, ReadOnly] private GameObject[] collisions;
    [SerializeField, ReadOnly] private int[] bottomHeights;

    protected readonly Transform[] stackTransforms = new Transform[3];
    protected readonly Renderer[] stackRenderers = new Renderer[3];

    private Volatile<Vector3> piecePosition = new(Vector3.zero);
    private Volatile<int> pieceOrientation = new(0);

    protected (int x, int z, int bottom)[] PieceBottom =>
    stackTransforms
        .Select(t => {
            Vector3 p = transform.TransformPoint(t.localPosition) + piecePosition.Value - transform.position;
            return (
                x: (int) p.x,
                z: (int) p.z,
                bottom: (int)(p.y - t.localScale.y) + 1
            );
        })
        .OrderBy(e => e.x)
        .ThenBy(e => e.z)   
        .ToArray();

    #region MonoBehavior
    protected virtual void Awake()
    {
        InterfaceRegistry<IPieceCollidable>.Register(this);
        piecePosition.Value = transform.position;

        for (int i = 0; i < 3; i++)
        {
            // Create the cube primitive
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Parent it under this Piece
            cube.transform.parent = transform;

            // Position it in X spaced apart, and start at Y=0, Z=0
            cube.transform.localPosition = new(i - 1, 0, 0); // positions: -1,0,1 on X

            // Get the references
            stackTransforms[i] = cube.transform;
            stackRenderers[i] = cube.GetComponent<Renderer>();
        }
        LinkComponents();
        ResetHeights(stackHeights);
    }
    protected virtual void Update()
    {
        bottomHeights = PieceBottom.Select(t => t.bottom).ToArray();
        collisions = InterfaceRegistry<IPieceCollidable>.All
            .Where(t => (Object)t != this && t.IsCollidedWithPiece(PieceBottom))
            .Select(t => ((MonoBehaviour)t).gameObject)
            .ToArray();
        illegal = collisions.Any();
        if (illegal) return;

        transform.eulerAngles = new(0f, (pieceOrientation.Value * 90) % 360, 0f);
        transform.position = piecePosition.Value;
    }
    #endregion

    #region IPieceCollidable
    public bool IsCollidedWithPiece((int x, int z, int bottom)[] pieceBottoms) =>
        stackTransforms.Any(t => t.localScale.y > 0) && pieceCollisionEnabled &&
        pieceBottoms.Any(collider => PieceBottom.Any(collidee =>
            collider.x == collidee.x && collider.z == collidee.z &&
            collider.bottom < transform.position.y
        ));

    public void SetPieceCollisionEnabled(bool isEnabled) => pieceCollisionEnabled = isEnabled;
    #endregion

    #region IGrabbable
    public void SetTransform(Vector3? position, int? orientation)
    {
        if (position.HasValue) piecePosition.Value = position.Value;
        if (orientation.HasValue) pieceOrientation.Value = orientation.Value;
    }
    public bool CanBeMoved { get; private set; }
    public int Orientation => pieceOrientation.Value;
    public Vector3 Position => piecePosition.Value;
    #endregion IGrabbable

    #region IHandlePieceFitEvent
    public PieceFitEventHandler Handler => new(
        ((Piece piece, GameObject gameObject) payload) =>
        {
            if (payload.piece == this && heightResettable) ResetHeights();
        });
    #endregion

    public void ResetHeights(Vector3Int Heights = default)
    {
        int[] heights = Heights == Vector3.zero?
            Enumerable.Range(0, 3)
                .Select(_ => Random.Range(minGeneratedHeight, maxGeneratedHeight + 1))
                .ToArray() :
            new int[] {Heights.x, Heights.y, Heights.z};

        for(int i = 0; i < 3; i++)
        {
            stackTransforms[i].localScale    = new Vector3(1, heights[i], 1);
            stackTransforms[i].localPosition = new Vector3(stackTransforms[i].localPosition.x, -heights[i] / 2f, stackTransforms[i].localPosition.z);
        }
        stackHeights = new Vector3Int(heights[0], heights[1], heights[2]);
    }

    public void LinkComponents()
    {
        
        foreach(IRequirePieceInfo component in GetComponents<IRequirePieceInfo>())
        foreach(PropertyInfo prop in component.GetType().GetProperties().Where(prop => prop.CanWrite))
        {
            if(prop.Name == "PieceBottom"       && prop.PropertyType == typeof(System.Func<(int, int, int)[]>))
                prop.SetValue(component, (System.Func<(int x, int z, int bottom)[]>)  (() => PieceBottom));
            if (prop.Name == "StackTransforms"  && prop.PropertyType == typeof(System.Func<Transform[]>))
                prop.SetValue(component, (System.Func<Transform[]>)        (() => stackTransforms));
        }
        
    }
}