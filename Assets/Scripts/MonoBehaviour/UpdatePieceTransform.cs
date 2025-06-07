using System;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

public class UpdatePieceTransform : MonoBehaviour
{
    [SerializeField] private Vector3Int stackHeights = Vector3Int.zero;
    [SerializeField] private TrackingInputInterface input;
    [SerializeField] private GameObject inputManager;
    [SerializeField] private Vector3 piecePosition;
    [SerializeField] private CollisionChecker[] checkers;

    private void Awake()
    {
        if (inputManager == null) inputManager = GameObject.Find("TrackingInputManager");
        if (inputManager == null) throw new MissingReferenceException("Cannot find required tracking input manager");
        input = inputManager.GetComponent<TrackingInputInterface>();

        checkers = GetComponentsInChildren<CollisionChecker>();

        ResetStack();
    }

    private void Update()
    {
        Vector3 oldAngle = transform.eulerAngles;
        Vector3 oldPosition = transform.position;

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
        piecePosition = transform.position;

        foreach(CollisionChecker checker in checkers)
            if (checker.IsCollided())
            {
                transform.position = oldPosition;
                transform.eulerAngles = oldAngle;
                return;
            }
        
    }

    public void ResetStack()
    {
        ModifyStackHeight[] modifiers = GetComponentsInChildren<ModifyStackHeight>();
        if (modifiers.Length != 3) throw new InvalidOperationException("Invalid number of stacks found. Requires 3.");

        stackHeights = new Vector3Int(
            modifiers[0].ResetHeight(stackHeights.x),
            modifiers[1].ResetHeight(stackHeights.y),
            modifiers[2].ResetHeight(stackHeights.z)
        );
    }

    public Vector3 StackHeights => stackHeights;
}
