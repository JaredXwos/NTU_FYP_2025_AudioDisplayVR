using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour, ICanCollideWithPiece
{
    [Header("Stack Configuration")]
    [SerializeField] private int maxGeneratedHeight = 3;
    [SerializeField] private int minGeneratedHeight = 1;

    [Header("Target Transform")]
    [SerializeField] private Vector3 piecePosition = Vector3.zero;
    [SerializeField] private int pieceOrientation = 0;

    [Header("Collision Information")]
    [Tooltip("Legality of target transform. Actual transform defaults to last legal transform.")]
    [SerializeField] private bool illegal;
    [SerializeField] private bool pieceCollisionEnabled = true;

    private readonly Transform[] stackTransforms = new Transform[3];
    private readonly Renderer[] stackRenderers = new Renderer[3];
    private readonly object transformLock = new();

    private (int x, int z, int bottom)[] PieceBottom => 
        stackTransforms
            .Select(t => (
                x:      (int) t.position.x,
                z:      (int) t.position.z,
                bottom: (int) (t.position.y - t.localScale.y)
            ))
            .ToArray();

    private void Awake()
    {
        InterfaceRegistry<ICanCollideWithPiece>.Register(this);

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

    private void Update()
    {
        illegal = InterfaceRegistry<ICanCollideWithPiece>.All.Any(t => t.CollidedWithPiece(PieceBottom));
        if (illegal) return;

        lock (transformLock)
        {
            transform.eulerAngles = new(0f, (pieceOrientation * 90) % 360, 0f);
            transform.position = piecePosition;
        }
    }

    public bool CollidedWithPiece((int x, int z, int bottom)[] pieceBottoms) =>
        stackTransforms.Any(t => t.localScale.y > 0) && pieceCollisionEnabled &&
        pieceBottoms.Any(collider => PieceBottom.Any(collidee =>
            collider.x == collidee.x && collider.z == collidee.z &&
            collider.bottom < transform.position.y
        ));

    public void SetPieceCollisionEnabled(bool isEnabled) => pieceCollisionEnabled = isEnabled;

    public void ResetHeights()
    {
        foreach(Transform t in transform)
        {
            int stackHeight = Random.Range(minGeneratedHeight, maxGeneratedHeight + 1);
            t.localScale =      new Vector3(1,                  stackHeight,        1                );
            t.localPosition =   new Vector3(t.localPosition.x, -stackHeight / 2f,   t.localPosition.z);
        }
    }
    public void SetPieceTransform(Vector3? position, int? orientation)
    {
        lock (transformLock)
        {
            if (position.HasValue) piecePosition = position.Value;
            if (orientation.HasValue) pieceOrientation = orientation.Value;
        }
    }
}