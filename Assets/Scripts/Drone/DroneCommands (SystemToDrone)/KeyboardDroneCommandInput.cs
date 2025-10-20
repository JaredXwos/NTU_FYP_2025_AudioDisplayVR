using UnityEngine;

public class KeyboardDroneInput : MonoBehaviour, IDroneCommandInput
{
    [Header("Keyboard Control Settings")]
    public float maxHorizVel = 3f;      // m/s horizontal velocity
    public float maxVertVel = 1.5f;    // m/s climb or descent
    public float yawRate = 1.0f;    // rad/s yaw rate
    public float baseAltitude = 40f;    // meters starting altitude
    public float velToTiltGain = 0.8f;  // theta = Kv * v / g
    public float maxTiltDeg = 25f;    // degrees tilt limit

    private float yaw;        // current heading in radians
    private float altitude;   // target altitude
    private const float g = 9.81f;

    void Awake()
    {
        yaw = 0f;
        altitude = baseAltitude;
    }

    public DroneCommand GetCommand()
    {
        // 1. Read input in the drone's local frame (FPV style)
        float right = 0f;
        float forward = 0f;
        float climb = 0f;

        if (Input.GetKey(KeyCode.W)) forward += 1f;
        if (Input.GetKey(KeyCode.S)) forward -= 1f;
        if (Input.GetKey(KeyCode.A)) right += 1f;
        if (Input.GetKey(KeyCode.D)) right -= 1f;
        if (Input.GetKey(KeyCode.R)) climb += 1f;
        if (Input.GetKey(KeyCode.F)) climb -= 1f;

        // Normalize to avoid faster diagonal movement
        Vector3 input = new Vector3(right, climb, forward).normalized;

        // 2. Convert to desired body-frame velocities
        float vx_body = input.x * maxHorizVel;
        float vz_body = input.z * maxHorizVel;
        float vy_body = input.y * maxVertVel;

        // 3. Update yaw heading (Q and E keys)
        if (Input.GetKey(KeyCode.E)) yaw -= yawRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) yaw += yawRate * Time.deltaTime;

        // Keep yaw within -pi to +pi
        yaw = Mathf.Repeat(yaw + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;

        // 4. Integrate altitude using climb velocity
        altitude += vy_body * Time.deltaTime;

        // 5. Map body velocities to tilt angles
        // Pitch (theta) controls forward/back motion
        // Roll  (phi) controls left/right motion
        float theta = Mathf.Clamp(vz_body / g * velToTiltGain,
                                  -maxTiltDeg * Mathf.Deg2Rad,
                                   maxTiltDeg * Mathf.Deg2Rad);
        float phi = Mathf.Clamp(-vx_body / g * velToTiltGain,
                                  -maxTiltDeg * Mathf.Deg2Rad,
                                   maxTiltDeg * Mathf.Deg2Rad);

        // 6. Return the command: roll, pitch, yaw, altitude
        return new DroneCommand(phi, theta, yaw, altitude);
    }

    public bool IsActive() => true;
}