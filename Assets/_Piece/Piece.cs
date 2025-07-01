using System.Linq;
using Unity.Collections;
using UnityEngine;

public class Piece : MonoBehaviour, IPieceCollidable, IGrabbable
{
    [Header("Stack Configuration")]
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;

    [Header("Target Transform")]
    [SerializeField] private Volatile<Vector3> piecePosition = new(Vector3.zero);
    [SerializeField] private Volatile<int> pieceOrientation = new(0);

    [Header("Collision Information")]
    [SerializeField] private bool pieceCollisionEnabled = true;
    [Tooltip("Legality of target transform. Actual transform defaults to last legal transform.")]
    [SerializeField] private volatile bool illegal;
    [SerializeField, ReadOnly] private GameObject[] collisions;
    [SerializeField, ReadOnly] private int[] bottomHeights;

    protected readonly Transform[] stackTransforms = new Transform[3];
    protected readonly Renderer[] stackRenderers = new Renderer[3];

    protected (int x, int z, int bottom)[] PieceBottom =>
    stackTransforms
        .Select(t => (
            x: (int)(piecePosition.Value.x + t.localPosition.x),
            z: (int)(piecePosition.Value.z + t.localPosition.z),
            bottom: (int)(piecePosition.Value.y - t.localScale.y)
        ))
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

        ResetHeights();
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

    public void ResetHeights()
    {
        foreach(Transform t in transform)
        {
            int stackHeight = Random.Range(minGeneratedHeight, maxGeneratedHeight + 1);
            t.localScale =      new Vector3(1,                  stackHeight,        1                );
            t.localPosition =   new Vector3(t.localPosition.x, -stackHeight / 2f,   t.localPosition.z);
        }
    }
}