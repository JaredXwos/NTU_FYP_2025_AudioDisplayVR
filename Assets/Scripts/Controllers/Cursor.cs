using Leap;
using UnityEngine;

public class Cursor : Dispatch
{
    [Header("Grabbing")]
    [SerializeField] private bool grabEnabled;
    [SerializeField] private string GrabbedName = string.Empty;

    [Header("Input Interface")]
    [SerializeField] private InputInterface input;

    private (int rot, Vector3 pos, IGrabbable item)? grabbed = null;

    protected override void Awake()
    {
        EventType = typeof(GrabEvent);
        PayloadType = typeof(GrabPayload);
        base.Awake();

        Check.PropertyEnabledElseAssign<InputInterface>(this, "input");
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Update()
    {
        transform.position = new Vector3(
            Mathf.RoundToInt(input.PiecePosition.x),
            Mathf.RoundToInt(input.PiecePosition.y),
            Mathf.RoundToInt(input.PiecePosition.z)
        );
        transform.localEulerAngles = new Vector3(0, 0, input.PieceOrientation * 90);
        if (grabEnabled)
        {
            if (input.IsGrabbing)
            {
                if (grabbed.HasValue)
                    grabbed.Value.item.SetTransform(transform.position + grabbed.Value.pos, input.PieceOrientation + grabbed.Value.rot);

                else if (
                    World.CheckCollision(Vector3Int.RoundToInt(input.PiecePosition), out CoreComponent collided) &&
                    collided is IGrabbable grabbable &&
                    grabbable.CanBeMoved
                ){
                    grabbed = (
                        grabbable.Orientation - input.PieceOrientation,
                        grabbable.Position - transform.position,
                        grabbable
                    );
                    GrabbedName = collided.name;
                    Invoke(new GrabPayload(collided, true));
                }
            }
            else
            {
                grabbed = null;
                GrabbedName = string.Empty;
            }
        }
    }
}