using System;
using UnityEngine;

public class UpdatePieceTransform : MonoBehaviour
{
    [SerializeField] private Vector3Int stackHeights = Vector3Int.zero;
    
    [SerializeField] private GameObject inputManager;
    [SerializeField] private bool illegal;
    [SerializeField] private Vector3 basetop;
    [SerializeField] private Vector3 piecebottom;

    private ModifyStackHeight[] modifiers;
    private TrackingInputInterface input;

    private static readonly int[][] baseHeights = new int[][]{
        new int[] {3, 2, 3, 1},
        new int[] {3, 1, 3, 3},
        new int[] {2, 2, 1, 1},
        new int[] {1, 1, 1, 2}
    };

    private void Awake()
    {
        if (inputManager == null) inputManager = GameObject.Find("TrackingInputManager");
        if (inputManager == null) throw new MissingReferenceException("Cannot find required tracking input manager");
        input = inputManager.GetComponent<TrackingInputInterface>();

        modifiers = GetComponentsInChildren<ModifyStackHeight>();
        if (modifiers.Length != 3) throw new InvalidOperationException("Invalid number of stacks found. Requires 3.");
        Array.Sort(modifiers, (a, b) => a.sortIndex.CompareTo(b.sortIndex));

        ResetStack();
    }

    private void Update()
    {
        Vector3 nextPosition = new(
            (int)input.PiecePosition.x,
            (int)input.PiecePosition.y,
            (int)input.PiecePosition.z
        );
        int nextOrientation = input.PieceOrientation;
        illegal = IsCollided(nextPosition, nextOrientation);
        if (illegal) return;

        transform.eulerAngles = new Vector3(
            0f, 
            (input.PieceOrientation * 90) % 360, 
            0f
        );

        transform.position = new Vector3(
            (int) input.PiecePosition.x, 
            (int) input.PiecePosition.y, 
            (int) input.PiecePosition.z
        );
    }

    private bool IsCollided(Vector3 position, int orientation)
    {
        basetop = Vector3.zero;
        int x = (int)position.x;
        int z = (int)position.z;

        switch (orientation % 4)
        {
            case 0:
                basetop = new Vector3(
                    (z >= 0 && z < baseHeights.Length && x - 1 >= 0 && x - 1 < baseHeights[z].Length) ? baseHeights[z][x - 1] : 0,
                    (z >= 0 && z < baseHeights.Length && x >= 0 && x < baseHeights[z].Length) ? baseHeights[z][x] : 0,
                    (z >= 0 && z < baseHeights.Length && x + 1 >= 0 && x + 1 < baseHeights[z].Length) ? baseHeights[z][x + 1] : 0
                );
                break;

            case 3:
                basetop = new Vector3(
                    (z - 1 >= 0 && z - 1 < baseHeights.Length && x >= 0 && x < baseHeights[z - 1].Length) ? baseHeights[z - 1][x] : 0,
                    (z >= 0 && z < baseHeights.Length && x >= 0 && x < baseHeights[z].Length) ? baseHeights[z][x] : 0,
                    (z + 1 >= 0 && z + 1 < baseHeights.Length && x >= 0 && x < baseHeights[z + 1].Length) ? baseHeights[z + 1][x] : 0
                );
                break;

            case 2:
                basetop = new Vector3(
                    (z >= 0 && z < baseHeights.Length && x + 1 >= 0 && x + 1 < baseHeights[z].Length) ? baseHeights[z][x + 1] : 0,
                    (z >= 0 && z < baseHeights.Length && x >= 0 && x < baseHeights[z].Length) ? baseHeights[z][x] : 0,
                    (z >= 0 && z < baseHeights.Length && x - 1 >= 0 && x - 1 < baseHeights[z].Length) ? baseHeights[z][x - 1] : 0
                );
                break;

            case 1:
                basetop = new Vector3(
                    (z + 1 >= 0 && z + 1 < baseHeights.Length && x >= 0 && x < baseHeights[z + 1].Length) ? baseHeights[z + 1][x] : 0,
                    (z >= 0 && z < baseHeights.Length && x >= 0 && x < baseHeights[z].Length) ? baseHeights[z][x] : 0,
                    (z - 1 >= 0 && z - 1 < baseHeights.Length && x >= 0 && x < baseHeights[z - 1].Length) ? baseHeights[z - 1][x] : 0
                );
                break;
        }
        piecebottom = new Vector3(position.y + 1, position.y + 1, position.y + 1) - stackHeights;
        return 
            piecebottom.x - basetop.x < 0 ||
            piecebottom.y - basetop.y < 0 ||
            piecebottom.z - basetop.z < 0;
    }

    public void ResetStack() => stackHeights = new Vector3Int(
        modifiers[0].ResetHeight(),
        modifiers[1].ResetHeight(),
        modifiers[2].ResetHeight()
    );
    

    public Vector3 StackHeights => stackHeights;
}
