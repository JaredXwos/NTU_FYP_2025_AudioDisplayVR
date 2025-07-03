using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class BroadcastingPiece : Piece, IToneInputProvider
{
    [SerializeField, ReadOnly] private int[] currentToneInput = new int[3] {-1, -1, -1};

    private readonly Volatile<int[]> _currentToneInput = new(new int[3]);

    #region MonoBehavior
    protected override void Update()
    {
        base.Update();
        
        Vector3 downward = transform.TransformDirection(Vector3.down);
        Vector3[] startPoint = PieceBottom
            .Select(p => new Vector3(p.x, p.bottom, p.z) - downward * 0.1f)
            .ToArray();

        for (int i = 0; i < 3; i++)
            if (Physics.Raycast(startPoint[i], downward, out RaycastHit hit, 10f))
                currentToneInput[i] = Mathf.FloorToInt(hit.distance);
            else
            {
                Array.Fill(currentToneInput, -1);
                break;
            }

        _currentToneInput.Value = currentToneInput;
    }
    #endregion

    #region IToneInputProvider
    public int[] GetToneInput() => _currentToneInput.Value;
    #endregion
}