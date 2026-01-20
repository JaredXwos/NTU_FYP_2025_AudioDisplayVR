using UnityEngine;
[DefaultExecutionOrder(1000)]
public class KeyboardDroneInput : MonoBehaviour, IDroneCommandInput
{
    [SerializeField] private IHasVelocity velocity;

    [Header("General")]
    [SerializeField] private float maxVelocity;

    [Header("Pitch: Local Z Direction")]
    [SerializeField] private float localForwardAccelerationTilt;
    [SerializeField] private float localBreakingTilt;
    [SerializeField] private bool isLocalForwardAccelerating;
    [SerializeField] private bool isBreaking;

    [Header("Yaw: Global Y Direction")]
    [SerializeField] private float yawRate;
    [SerializeField] private float yawAngle;

    [Header("Roll: Local X Direction")]
    [SerializeField] private float localRollBalancingCoefficient;
    [SerializeField] private float localRollCorrection;
    [SerializeField] private float localStrafeTilt;

    [Header("DEBUG: Velocity Decomposition")]
    [SerializeField] private bool debugVelocity = true;
    [SerializeField] private float debugRayScale = 0.25f;

    // Debug scalars (visible in Inspector)
    [SerializeField] private float vRight;
    [SerializeField] private float vUp;
    [SerializeField] private float vForward;

    private const KeyCode LocalForward = KeyCode.W;
    private const KeyCode LocalBreak = KeyCode.S;
    private const KeyCode YawCounterclockwise = KeyCode.Q;
    private const KeyCode YawClockwise = KeyCode.E;
    private const KeyCode LocalStrafeLeft = KeyCode.A;
    private const KeyCode LocalStrafeRight = KeyCode.D;

    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IHasVelocity>(this, "velocity");
    }

    private void Update()
    {
        if (velocity == null) return;

        float dt = Time.deltaTime;

        // --- Yaw control (GLOBAL Y) ---
        if (Input.GetKey(YawCounterclockwise))
            yawAngle += yawRate * dt;
        else if (Input.GetKey(YawClockwise))
            yawAngle -= yawRate * dt;

        // --- Forward/back control (LOCAL Z) ---
        isLocalForwardAccelerating = Input.GetKey(LocalForward);
        isBreaking = Input.GetKey(LocalBreak);

        // --- Velocity decomposition in BODY axes ---
        Vector3 vWorld = velocity.Velocity;

        vRight = Vector3.Dot(vWorld, transform.right);    // LOCAL X
        vUp = Vector3.Dot(vWorld, transform.up);       // LOCAL Y
        vForward = Vector3.Dot(vWorld, transform.forward);  // LOCAL Z

        // --- Roll braking control (LOCAL X) ---
        float vLat = vRight;

        localRollCorrection = Mathf.Clamp(
            -localRollBalancingCoefficient * vLat,
            -0.5f,
            0.5f
        );

        if(Input.GetKey(LocalStrafeLeft))
            localRollCorrection -= localStrafeTilt;
        else if(Input.GetKey(LocalStrafeRight))
            localRollCorrection += localStrafeTilt;
    }

    public DroneCommand GetCommand()
    {
        return new DroneCommand(
            localRollCorrection,                          // Phi (roll, Z axis)
            isLocalForwardAccelerating
                ? localForwardAccelerationTilt
                : isBreaking
                    ? localBreakingTilt
                    : 0f,                                     // Theta (pitch, X axis)
            yawAngle,                                     // Psi (yaw, Y axis)
            0f
        );
    }

    public bool IsActive() => true;

    private void OnDrawGizmos()
    {
        if (!debugVelocity || velocity == null) return;

        Vector3 p = transform.position;
        Vector3 vWorld = velocity.Velocity;

        // Total world velocity (white)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(p, p + vWorld * debugRayScale);

        // Body-axis components
        Gizmos.color = Color.red;    // right (X)
        Gizmos.DrawLine(p, p + debugRayScale * vRight * transform.right);

        Gizmos.color = Color.green;  // up (Y)
        Gizmos.DrawLine(p, p + debugRayScale * vUp * transform.up);

        Gizmos.color = Color.blue;   // forward (Z)
        Gizmos.DrawLine(p, p + debugRayScale * vForward * transform.forward);
    }
}