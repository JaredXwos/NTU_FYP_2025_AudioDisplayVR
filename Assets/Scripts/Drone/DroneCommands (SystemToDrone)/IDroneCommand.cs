using System;

public readonly struct DroneCommand
{
    public float Roll { get; }
    public float Pitch { get; }
    public float Yaw { get; }
    public float Altitude { get; }

    public DroneCommand(float roll, float pitch, float yaw, float altitude)
    {
        // Validate and clamp ranges
        Roll = Math.Clamp(roll, -0.5f, 0.5f);     // ±30 degrees
        Pitch = Math.Clamp(pitch, -0.5f, 0.5f);    // ±30 degrees
        Yaw = NormalizeYaw(yaw);                 // wrap to -pi…+pi
        Altitude = Math.Clamp(altitude, 0f, 100f);    // safe altitude band
    }

    private static float NormalizeYaw(float yaw)
    {
        // Wrap angle into [-pi, +pi]
        while (yaw > MathF.PI) yaw -= 2 * MathF.PI;
        while (yaw < -MathF.PI) yaw += 2 * MathF.PI;
        return yaw;
    }
}