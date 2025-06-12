public delegate double[] ChordGenerator(double frequency);
public static class Chord
{
    // -------------------------------------------------------------------------
    // BASE CHORDS
    // -------------------------------------------------------------------------
    public static readonly ChordGenerator Silence = freq =>
        new double[] { 0, 0, 0 };

    public static readonly ChordGenerator SameNoteOver3Octaves = freq =>
        new double[] { freq, 2 * freq, 4 * freq };

    public static readonly ChordGenerator ExtendedConsonantHarmonics = freq =>
        new double[] { freq * 1.0, freq * 2.0, freq * 3.0, freq * 4.0, freq * 6.0 };

    // -------------------------------------------------------------------------
    // TRADITIONAL TRIADS
    // -------------------------------------------------------------------------
    public static readonly ChordGenerator MajorTriadJustIntonation = freq =>
        new double[] { freq, freq * 5.0 / 4.0, freq * 3.0 / 2.0 };

    public static readonly ChordGenerator MajorTriadWideJustIntonation = freq =>
        new double[] { freq, freq * 5.0 / 2.0 * 2.0, freq * 6.0 };

    public static readonly ChordGenerator MinorTriadJustIntonation = freq =>
        new double[] { freq, freq * 6.0 / 5.0, freq * 3.0 / 2.0 };

    // -------------------------------------------------------------------------
    // SUSPENDED TRIADS
    // -------------------------------------------------------------------------
    public static readonly ChordGenerator Sus2TriadJustIntonation = freq =>
        new double[] { freq, freq * 9.0 / 8.0, freq * 3.0 / 2.0 };

    public static readonly ChordGenerator Sus4TriadJustIntonation = freq =>
        new double[] { freq, freq * 4.0 / 3.0, freq * 3.0 / 2.0 };

    // -------------------------------------------------------------------------
    // ALTERED TRIADS
    // -------------------------------------------------------------------------
    public static readonly ChordGenerator DiminishedTriadJustIntonation = freq =>
        new double[] { freq, freq * 6.0 / 5.0, freq * 45.0 / 32.0 };

    public static readonly ChordGenerator AugmentedTriadJustIntonation = freq =>
        new double[] { freq, freq * 5.0 / 4.0, freq * 25.0 / 16.0 };

    // -------------------------------------------------------------------------
    // QUARTAL TRIADS
    // -------------------------------------------------------------------------
    public static readonly ChordGenerator QuartalTriadJustIntonation = freq =>
        new double[] { freq, freq * 4.0 / 3.0, freq * 16.0 / 9.0 };
}