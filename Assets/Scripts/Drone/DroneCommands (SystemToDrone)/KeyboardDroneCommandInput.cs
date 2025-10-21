using UnityEngine;

public class KeyboardDroneInput : MonoBehaviour, IDroneCommandInput
{
    [SerializeField] private IHasVelocity velocity;
    [SerializeField] private Vector3 desiredVelocity;
    [SerializeField] private float maxVelocity;
    [SerializeField] private Vector3 Velocity;


    [SerializeField] private float velocityDrop;
    [SerializeField] private float velocityGain;
    [SerializeField] private float controlSensitivity = 0.2f;

    [Header("Tilt Limits")]
    [SerializeField] private float maxTiltUser = 0.5f;     // radians or normalized tilt (user input)
    [SerializeField] private float maxTiltCorrection = 1f; // stronger corrective tilt

    [SerializeField] private float maxYawRate = 1.5f;
    [SerializeField] private float yawAngle;

    [SerializeField] private float accelerationGain;
    [SerializeField] private float damping;
    [SerializeField] private float timePerCommand;

    private void Awake()
    {
        Check.PropertyEnabledElseAssign<IHasVelocity>(this, "velocity");
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // --- Forward/back control (Z-axis) ---
        desiredVelocity.z = UpdateAxis(
            desiredVelocity.z,
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            velocityGain,
            velocityDrop,
            dt);

        // --- Left/right control (X-axis) ---
        desiredVelocity.x = UpdateAxis(
            desiredVelocity.x,
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.A),
            velocityGain,
            velocityDrop,
            dt);

        // --- Clamp final velocity magnitude ---
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxVelocity);

        // --- Yaw control (instant) ---
        float yawInput = 0f;
        if (Input.GetKey(KeyCode.Q)) yawInput -= 1f;
        if (Input.GetKey(KeyCode.E)) yawInput += 1f;

        // Accumulate yaw heading (degrees)
        yawAngle += yawInput * maxYawRate * dt * Mathf.Rad2Deg;

        // Wrap yaw angle to avoid overflow
        if (yawAngle > Mathf.PI * 2) yawAngle -= Mathf.PI*2;
        else if (yawAngle < -Mathf.PI * 2) yawAngle += Mathf.PI * 2;

        // Rotate desired velocity into heading space
        Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
        desiredVelocity = yawRotation * new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
    }

    public DroneCommand GetCommand()
    {
        Vector3 currentVelocity = transform.InverseTransformDirection(Velocity);
        Vector3 error = desiredVelocity - currentVelocity;

        float roll = ComputeTilt(error.x, currentVelocity.x);
        float pitch = ComputeTilt(error.z, currentVelocity.z);
        float yaw = yawAngle;
        float altitude = 0f;
        UpdateVelocity(roll, pitch, timePerCommand);
        return new DroneCommand(roll, pitch, yaw, altitude);
    }

    private void UpdateVelocity(float roll, float pitch, float deltaTime)
    {
        // 1. Convert roll/pitch (radians) to local-frame acceleration vector.
        // Positive pitch (nose up) accelerates backward in body frame.
        // Positive roll (right wing down) accelerates right in body frame.
        // Using sin(theta) * g  lateral acceleration component from tilt.
        Vector3 localAccel = new Vector3(
            Mathf.Sin(roll),   // right (+X)
            0f,                               // no direct vertical accel here
            Mathf.Sin(pitch)  // forward (+Z)
        ) * accelerationGain;

        // 2. Rotate local acceleration into world/global frame using current orientation.
        Vector3 worldAccel = transform.rotation * localAccel;

        // 3. Integrate acceleration over time to get change in velocity.
        Vector3 deltaV = worldAccel * deltaTime;

        // 4. Apply change to current velocity.
        Velocity += deltaV;

        // 5. Apply simple exponential damping (drag / air resistance).
        //    v_new = v_old * (1 - damping * dt)   exp(-damping * dt)
        Velocity *= Mathf.Exp(-damping * deltaTime);
    }

    public bool IsActive() => true;

    private static float UpdateAxis(
    float current,
    bool positiveHeld,
    bool negativeHeld,
    float gain,
    float dropTime,
    float deltaTime)
    {
        if (positiveHeld)
            current += gain * deltaTime;
        else if (negativeHeld)
            current -= gain * deltaTime;
        else
            current = Mathf.MoveTowards(current, 0f, (gain / dropTime) * deltaTime);

        return current;
    }

    private float ComputeTilt(float error, float current)
    {
        // Convert velocity error to tilt using proportional control
        float tilt = error * controlSensitivity;

        // Determine if we're accelerating (same direction) or decelerating (opposite)
        bool sameDirection = Mathf.Sign(error) == Mathf.Sign(current);

        // Select appropriate clamp
        float maxTilt = sameDirection ? maxTiltUser : maxTiltCorrection;

        return Mathf.Clamp(tilt, -maxTilt, maxTilt);
    }
}