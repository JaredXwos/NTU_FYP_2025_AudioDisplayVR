using System.Threading.Tasks;
using UnityEngine;
public class SimulatedInputInterface : TrackingInputInterface
{
    [SerializeField] int sensitivity = 10;
    private int[] counters = new int[8];
    private static readonly KeyCode[] keys = new KeyCode[]{ KeyCode.Q, KeyCode.E, KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.LeftShift };
    protected override async void BackgroundUpdate() {
        while(!token.IsCancellationRequested)
        lock (_lock)
        {
            if (counters[0] > sensitivity) { _pieceOrientation++; counters[0] = 0; }
            if (counters[1] > sensitivity) { _pieceOrientation--; counters[1] = 0; }
            if (counters[2] > sensitivity) { _piecePosition += new Vector3(0, 0, 1); counters[2] = 0; }
            if (counters[3] > sensitivity) { _piecePosition += new Vector3(0, 0, -1); counters[3] = 0; }
            if (counters[4] > sensitivity) { _piecePosition += new Vector3(-1, 0, 0); counters[4] = 0; }
            if (counters[5] > sensitivity) { _piecePosition += new Vector3(1, 0, 0); counters[5] = 0; }
            if (counters[6] > sensitivity) { _piecePosition += new Vector3(0, 1, 0); counters[6] = 0; }
            if (counters[7] > sensitivity) { _piecePosition += new Vector3(0, -1, 0); counters[7] = 0; }
        }
    }
    private void Update()
    {
        for(int i = 0; i < 8; i++) counters[i] += Input.GetKey(keys[i])? 1 : 0;
    }
}