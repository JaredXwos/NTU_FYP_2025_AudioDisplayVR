using Leap;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LeapServiceProvider))]
public class TrackingInputInterface : MonoBehaviour
{
    // Private value storage to store all the 3 parameters we want to extract from the tracker
    protected float _clockwiseMoment = 0.0f;

    protected int _lastYawZone = 0;
    protected int _pieceOrientation = 0;

    [SerializeField] protected Vector3 scalingFactor = new(40f, 30f, 30f);
    [SerializeField] protected Vector3 initialDisplacement = new(0f, -5f, 5f);
    protected Vector3 _piecePosition = Vector3.zero;

    private LeapServiceProvider leapProvider;
    private Frame _cachedFrame;

    protected CancellationTokenSource tokenSource;
    protected CancellationToken token;
    protected readonly object _lock = new();

    private void Awake()
    {
        if(leapProvider == null) leapProvider = GetComponent<LeapServiceProvider>();
        tokenSource = new();
        token = tokenSource.Token;
        Task.Run(BackgroundUpdate);
    }

    private void OnDestroy() => tokenSource.Cancel();

    private void Update() => _cachedFrame = leapProvider.CurrentFrame;

    protected virtual void BackgroundUpdate()
    {
        while (!token.IsCancellationRequested)
        {
            if (_cachedFrame == null) continue;
            Hand leftHand = _cachedFrame.Hands.Find(h => h.IsLeft);
            Hand rightHand = _cachedFrame.Hands.Find(h => !h.IsLeft);

            // Assign clockwise moment from the roll of the left hand, with 0 roll being palm down.
            // Vector 1, Vector 2, the plane of comparison
            if (leftHand != null) _clockwiseMoment = Vector3.SignedAngle(
                Vector3.down,
                leftHand.PalmNormal.normalized,
                leftHand.Direction.normalized
            );

            if (rightHand == null) continue;


            // Assign piece orientation from vertical pitch gestures
            int currentYawZone = Vector3.SignedAngle(
                Vector3.left,
                rightHand.PalmNormal.normalized,
                Vector3.up
            ) switch
            {
                <= -60f => +1,
                >= 60f => -1,
                _ => 0
            };


            if (_lastYawZone == 0 && currentYawZone != 0)
                lock(_lock) _pieceOrientation += currentYawZone;

            _lastYawZone = currentYawZone;


            // Apply scaling and displacement
            lock (_lock) _piecePosition = initialDisplacement + Vector3.Scale(
                rightHand.PalmPosition,
                scalingFactor
            );
        }
    }

    protected float _debugTimer = 0f;
    private const float _debugInterval = 0.5f;
    private void FixedUpdate()
    {
        _debugTimer += Time.fixedDeltaTime;

        if (_debugTimer >= _debugInterval)
        {
            PrintToDebug();
            _debugTimer = 0f;
        }
    }

    // Public getter functions
    public float ClockwiseMoment => _clockwiseMoment;
    public int PieceOrientation => _pieceOrientation;
    public Vector3 PiecePosition
    {
        get
        {
            lock (_lock) return _piecePosition;
        }
    }

    public void PrintToDebug()
    { 
        Debug.Log($"ClockwiseMoment: {ClockwiseMoment}, " +
                  $"PieceOrientation: {PieceOrientation}, " +
                  $"PiecePosition: {PiecePosition}");
    }
}

