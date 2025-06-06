using UnityEngine;

public static class Chord
{
    public static Vector3 Silence(float freq) => Vector3.zero;

    public static Vector3 SameNoteOver3Octaves(float freq) =>
        new(freq, 2 * freq, 4 * freq);
    // Traditional triads
    public static Vector3 MajorTriadJustIntonation(float freq) =>
        new(freq, freq * 5f / 4f, freq * 3f / 2f);

    public static Vector3 MajorTriadWideJustIntonation(float freq) =>
        new(freq, freq * 5f / 2f * 2f, freq * 6f);

    public static Vector3 MinorTriadJustIntonation(float freq) =>
        new(freq, freq * 6f / 5f, freq * 3f / 2f);

    // Suspended triads
    public static Vector3 Sus2TriadJustIntonation(float freq) => // Root, major 2nd, perfect 5th
        new(freq, freq * 9f / 8f, freq * 3f / 2f);

    public static Vector3 Sus4TriadJustIntonation(float freq) => // Root, perfect 4th, perfect 5th
        new(freq, freq * 4f / 3f, freq * 3f / 2f);

    // Altered triads
    public static Vector3 DiminishedTriadJustIntonation(float freq) => // Root, minor 3rd, diminished 5th
        new(freq, freq * 6f / 5f, freq * 45f / 32f); // 45/32 = diminished 5th

    public static Vector3 AugmentedTriadJustIntonation(float freq) => // Root, major 3rd, augmented 5th
        new(freq, freq * 5f / 4f, freq * 25f / 16f); // 25/16 = augmented 5th

    // Quartal triad (stacked perfect 4ths)
    public static Vector3 QuartalTriadJustIntonation(float freq) =>
        new(freq, freq * 4f / 3f, freq * 16f / 9f); // 4/3 and 16/9 = two stacked P4s
}