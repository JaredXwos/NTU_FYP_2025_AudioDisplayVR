using System;

public static class Anglef
{
    public static readonly float Tau = MathF.PI * 2;
    public static readonly float Epsilon = 1e-6f;
    public static float ZeroIfFloatZero(float angle)
     => MathF.Abs(angle) < Epsilon ? 0f : angle;
    public static float ClockwiseAngle(float from, float to)
        => ZeroIfFloatZero(((from - to) % Tau + Tau) % Tau);
    public static float CounterClockwiseAngle(float from, float to)
        => ZeroIfFloatZero(((to - from) % Tau + Tau) % Tau);
    public static float Normalize(float angle)
        => ((angle + MathF.PI) % Tau + Tau) % Tau - MathF.PI;
}
