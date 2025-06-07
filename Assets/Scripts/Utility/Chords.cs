using UnityEngine;

public static class Chord
{
    public static double[] Silence(double freq) => new double[] { 0, 0, 0 };

    public static double[] SameNoteOver3Octaves(double freq) =>
        new double[] { freq, 2 * freq, 4 * freq };

    public static double[] ExtendedConsonantHarmonics(double freq) => 
        new double[] {freq * 1.0, freq * 2.0,  freq * 3.0,  freq * 4.0, freq * 6.0};

    // Traditional triads
    public static double[] MajorTriadJustIntonation(double freq) =>
        new double[] { freq, freq * 5.0 / 4.0, freq * 3.0 / 2.0 };

    public static double[] MajorTriadWideJustIntonation(double freq) =>
        new double[] { freq, freq * 5.0 / 2.0 * 2.0, freq * 6.0 };

    public static double[] MinorTriadJustIntonation(double freq) =>
        new double[] { freq, freq * 6.0 / 5.0, freq * 3.0 / 2.0 };

    // Suspended triads
    public static double[] Sus2TriadJustIntonation(double freq) =>
        new double[] { freq, freq * 9.0 / 8.0, freq * 3.0 / 2.0 };

    public static double[] Sus4TriadJustIntonation(double freq) =>
        new double[] { freq, freq * 4.0 / 3.0, freq * 3.0 / 2.0 };

    // Altered triads
    public static double[] DiminishedTriadJustIntonation(double freq) =>
        new double[] { freq, freq * 6.0 / 5.0, freq * 45.0 / 32.0 };

    public static double[] AugmentedTriadJustIntonation(double freq) =>
        new double[] { freq, freq * 5.0 / 4.0, freq * 25.0 / 16.0 };

    // Quartal triad
    public static double[] QuartalTriadJustIntonation(double freq) =>
        new double[] { freq, freq * 4.0 / 3.0, freq * 16.0 / 9.0 };
}