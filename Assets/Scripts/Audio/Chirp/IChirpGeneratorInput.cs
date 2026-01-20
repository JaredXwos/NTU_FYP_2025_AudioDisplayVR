using Unity.Mathematics;

public readonly struct ChirpGeneratorInput
{
    public readonly int Duration;   // Duration of the chirp in frames
    public readonly float StartFreq; // Hz
    public readonly float EndFreq;   // Hz
    public readonly float2 Source;   // 2D position

    public ChirpGeneratorInput(int duration, float startFreq, float endFreq, float2 source)
    {
        if (
            math.isfinite(startFreq) &&
            math.isfinite(endFreq) &&
            math.isfinite(duration) &&  // overloads to double, safe cast
            math.all(math.isfinite(source)) &&

            duration >= 0 &&
            startFreq >= 0f &&
            endFreq >= 0f &&

            (startFreq > 0f && endFreq > 0f || startFreq == 0f && endFreq == 0f) // both zero or both non-zero
        )
        {
            Duration = duration;
            StartFreq = startFreq;
            EndFreq = endFreq;
            Source = source;
        }
        else
        {
            Duration = 0;
            StartFreq = 0f;
            EndFreq = 0f;
            Source = float2.zero;
            return;
        }
    }
    public override string ToString()
    {
        return $"ChirpGeneratorInput(Duration: {Duration}, StartFreq: {StartFreq}, EndFreq: {EndFreq}, Source: {Source})";
    }
}

public interface IChirpGeneratorInput
{
    ChirpGeneratorInput NextChirpInput();
}