using UnityEngine;
public class SimulatedInputInterface : InputInterface
{
    [SerializeField] int sensitivity = 10;
    private readonly int[] counters = new int[9];
    private static readonly KeyCode[] keys = new KeyCode[]{ 
        KeyCode.Q, KeyCode.E, 
        KeyCode.W, KeyCode.S, 
        KeyCode.A, KeyCode.D, 
        KeyCode.Space, KeyCode.LeftShift,
        KeyCode.Return
    };
    protected override void BackgroundUpdate() {
        while(!token.IsCancellationRequested)
        {
            if (counters[0] > sensitivity) { PieceOrientation++; counters[0] = 0; }
            if (counters[1] > sensitivity) { PieceOrientation--; counters[1] = 0; }
            if (counters[2] > sensitivity) { PiecePosition += new Vector3(0, 0, 1); counters[2] = 0; }
            if (counters[3] > sensitivity) { PiecePosition += new Vector3(0, 0, -1); counters[3] = 0; }
            if (counters[4] > sensitivity) { PiecePosition += new Vector3(-1, 0, 0); counters[4] = 0; }
            if (counters[5] > sensitivity) { PiecePosition += new Vector3(1, 0, 0); counters[5] = 0; }
            if (counters[6] > sensitivity) { PiecePosition += new Vector3(0, 1, 0); counters[6] = 0; }
            if (counters[7] > sensitivity) { PiecePosition += new Vector3(0, -1, 0); counters[7] = 0; }
        }
    }
    private void Update()
    {
        for(int i = 0; i < counters.Length; i++) counters[i] += Input.GetKey(keys[i])? 1 : 0;
        IsGrabbing = Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
    }
}