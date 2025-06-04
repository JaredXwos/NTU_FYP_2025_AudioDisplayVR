using UnityEngine;
using Leap;

[RequireComponent(typeof(LeapServiceProvider))]
public class TrackingInputInterface : MonoBehaviour
{
    // Private value storage to store all the 3 parameters we want to extract from the tracker
    [SerializeField] private float _clockwiseMoment = 0.0f;

    [SerializeField] private int _lastYawZone = 0;
    [SerializeField] private int _pieceOrientation = 0;

    [SerializeField] private Vector3 scalingFactor = new(40f, 30f, 30f);
    [SerializeField] private Vector3 initialDisplacement = new(0f, -5f, 5f);
    [SerializeField] private Vector3 _piecePosition = Vector3.zero;

    [SerializeField] private LeapServiceProvider leapProvider;

    private void Awake() => leapProvider = leapProvider != null ? leapProvider : GetComponent<LeapServiceProvider>();

    private void Update()
    {
        var frame = leapProvider.CurrentFrame;
        var leftHand = frame.Hands.Find(h => h.IsLeft);
        var rightHand = frame.Hands.Find(h => !h.IsLeft);

        // Assign clockwise moment from the roll of the left hand, with 0 roll being palm down.
        // Vector 1, Vector 2, the plane of comparison
        if(leftHand != null) _clockwiseMoment = Vector3.SignedAngle(
            Vector3.down,
            leftHand.PalmNormal.normalized,
            leftHand.Direction.normalized
        );

        if (rightHand == null) return;

        
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
            _pieceOrientation += currentYawZone;
        
        _lastYawZone = currentYawZone;

        
        // Apply scaling and displacement
        _piecePosition = initialDisplacement + Vector3.Scale(
            rightHand.PalmPosition,
            scalingFactor
        );
    }

    private float _debugTimer = 0f;
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
    public Vector3 PiecePosition => _piecePosition;

    public void PrintToDebug()
    {
        Debug.Log($"ClockwiseMoment: {ClockwiseMoment}, " +
                  $"PieceOrientation: {PieceOrientation}, " +
                  $"PiecePosition: {PiecePosition}");
    }
}
