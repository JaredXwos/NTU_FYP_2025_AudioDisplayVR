using Leap;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LeapServiceProvider))]
public class TrackingInputInterface : MonoBehaviour
{
    // LEAP MOTION INPUT DATA
    // -----------------------------------------------------------------------------
    protected Vector3 leftPalmNormalNormalised;
    protected Vector3 leftDirectionNormalised;
    protected Vector3 rightPalmNormalNormalised;
    protected Vector3 rightPalmPosition;

    protected bool leftHandExists;
    protected bool rightHandExists;

    protected readonly object inputLock = new();

    // INTERFACE OUTPUT DATA
    // -----------------------------------------------------------------------------
    [SerializeField] protected float _clockwiseMoment = 0.0f;
    [SerializeField] protected int _pieceOrientation = 0;
    [SerializeField] protected Vector3 _piecePosition = Vector3.zero;
        
    protected readonly object outputLock = new();

    // OUTPUT AND CONTROL PARAMETERS
    // -----------------------------------------------------------------------------
    [SerializeField] protected Vector3 scalingFactor = new(40f, 30f, 30f);
    [SerializeField] protected Vector3 initialDisplacement = new(0f, -5f, 5f);

    // LEAP MOTION SDK AND ASYNC CONTROL
    // -----------------------------------------------------------------------------
    protected LeapServiceProvider leapProvider;

    protected CancellationTokenSource tokenSource;  // This is to send the suicide instruction
    protected CancellationToken token;              // This is to receive the suicide instruction
    
    

    private void Awake()
    {
        if(leapProvider == null) leapProvider = GetComponent<LeapServiceProvider>();
        tokenSource = new();
        token = tokenSource.Token;
        Task.Run(BackgroundUpdate);
    }

    private void OnDestroy() => tokenSource.Cancel();

    private void Update(){
        Frame currentFrame = leapProvider.CurrentFrame;
        if (currentFrame == null) return;
        lock (inputLock)
        {
            Hand leftHand = currentFrame.Hands.Find(h => h.IsLeft);
            leftHandExists = leftHand != null;
            if (leftHandExists)
            {
                leftPalmNormalNormalised = leftHand.PalmNormal.normalized;
                leftDirectionNormalised = leftHand.Direction.normalized;
            }

            Hand rightHand = currentFrame.Hands.Find(h => !h.IsLeft);
            rightHandExists = rightHand != null;
            if (rightHandExists)
            {
                rightPalmNormalNormalised = rightHand.PalmNormal.normalized;
                rightPalmPosition = rightHand.PalmPosition;
            }
        }
    } 

    protected virtual void BackgroundUpdate()
    {
        int _lastRollZone = 0;
        while (!token.IsCancellationRequested)
        {
            // Assign clockwise moment from the roll of the left hand, with 0 roll being palm down.
            // Vector 1, Vector 2, the plane of comparison
            if (leftHandExists) 
                lock(inputLock)
                _clockwiseMoment = Vector3.SignedAngle(
                Vector3.down,
                leftPalmNormalNormalised,
                leftDirectionNormalised
            );

            if (!rightHandExists) continue;

            // Assign piece orientation from vertical pitch gestures
            // Split into roll zones
            int currentRollZone;
            lock (inputLock)
                currentRollZone = Vector3.SignedAngle(
                    Vector3.left,
                    rightPalmNormalNormalised,
                    Vector3.forward // roll is around the hand's forward direction
                ) switch
                {
                    <= -50f => +1, // rolled counter-clockwise past -50°
                    >= +50f => -1, // rolled clockwise past +50°
                    _ => 0
                };

            // Increment piece orientation on crossing thresholds
            if (_lastRollZone == 0 && currentRollZone != 0)
                lock (outputLock) _pieceOrientation += currentRollZone;

            _lastRollZone = currentRollZone;


            // Apply scaling and displacement
            lock (outputLock) lock(inputLock) _piecePosition = initialDisplacement + Vector3.Scale(
                rightPalmPosition,
                scalingFactor
            );
        }
    }

    // Public getter functions
    public float ClockwiseMoment => _clockwiseMoment;
    public int PieceOrientation => _pieceOrientation;
    public Vector3 PiecePosition
    {
        get
        {
            lock (outputLock) return _piecePosition;
        }
    }
}

