using Leap;
using UnityEngine;

[RequireComponent(typeof(LeapServiceProvider))]
public class LeapMotionInputInterface : InputInterface
{
    [Header("Interface Callibration Controls")]
    // -----------------------------------------------------------------------------

    [Tooltip("Ratio between 1 unit of length in the real world vs 1 unit of length in the game world")]
    [SerializeField] protected Vector3 scalingFactor = new(40f, 30f, 30f);

    [Tooltip("Where position origin is with respect to the game world")]
    [SerializeField] protected Vector3 initialDisplacement = new(0f, -5f, 5f);

    [Tooltip("Minimum strength to be considered a grab, ranges from 0 to 1")]
    [SerializeField] protected float grabStrength = 0.8f;

    public Vector3 RightPalmPosition = new();

    // LEAP MOTION INPUT DATA
    // -----------------------------------------------------------------------------
    private readonly Volatile<Vector3> leftPalmNormalNormalised = new();
    private readonly Volatile<Vector3> leftDirectionNormalised = new();
    private readonly Volatile<Vector3> rightPalmNormalNormalised = new();
    private readonly Volatile<Vector3> rightPalmPosition = new();

    private volatile bool leftHandExists;
    private volatile bool rightHandExists;

    protected LeapServiceProvider leapProvider;

    #region MonoBehavior
    protected override void Awake()
    {
        base.Awake();
        if(leapProvider == null) leapProvider = GetComponent<LeapServiceProvider>();
    }

    private void Update(){
        Frame currentFrame = leapProvider.CurrentFrame;
        if (currentFrame == null) return;

        Hand leftHand = currentFrame.Hands.Find(h => h.IsLeft);
        Hand rightHand = currentFrame.Hands.Find(h => !h.IsLeft);

        leftHandExists = leftHand != null;
        rightHandExists = rightHand != null;

        if (leftHandExists)
        {
            leftPalmNormalNormalised.Value = leftHand.PalmNormal.normalized;
            leftDirectionNormalised.Value = leftHand.Direction.normalized;
        }

        if (rightHandExists)
        {
            rightPalmNormalNormalised.Value = rightHand.PalmNormal.normalized;
            rightPalmPosition.Value = rightHand.PalmPosition;
            IsGrabbing = rightHand.GrabStrength > grabStrength;
        }
        
    }
    #endregion

    #region InputInterface
    protected override void BackgroundUpdate()
    {
        int _lastRollZone = 0;
        while (!token.IsCancellationRequested)
        {
            // Assign clockwise moment from the roll of the left hand, with 0 roll being palm down.
            // Vector 1, Vector 2, the plane of comparison
            if (leftHandExists)
                ClockwiseMoment = Vector3.SignedAngle(
                    Vector3.down,
                    leftPalmNormalNormalised.Value,
                    leftDirectionNormalised.Value
                );

            if (!rightHandExists) continue;

            // Assign piece orientation from vertical pitch gestures
            // Split into roll zones
            int currentRollZone;
            currentRollZone = Vector3.SignedAngle(
                Vector3.left,
                rightPalmNormalNormalised.Value,
                Vector3.forward // roll is around the hand's forward direction
            ) switch
            {
                <= -50f => +1, // rolled counter-clockwise past -50°
                >= +50f => -1, // rolled clockwise past +50°
                _ => 0
            };

            // Increment piece orientation on crossing thresholds
            if (_lastRollZone == 0 && currentRollZone != 0)
                PieceOrientation += currentRollZone;

            _lastRollZone = currentRollZone;


            // Apply scaling and displacement
            PiecePosition = initialDisplacement + Vector3.Scale(
                rightPalmPosition.Value,
                scalingFactor
            );
        }
    }
    #endregion
}

