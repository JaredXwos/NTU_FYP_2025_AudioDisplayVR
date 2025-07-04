using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class GroundSonar : MonoBehaviour, IRequirePieceInfo
{
    [SerializeField, ReadOnly] private int[] groundClearance = new int[3] {-1, -1, -1};

    private readonly Volatile<int[]> _groundClearance = new(new int[3]);
    public Piece Parent { get; private set; }

    #region MonoBehavior
    private void Awake() => Parent = GetComponent<Piece>();

    protected void Update()
    {
        Vector3 downward = transform.TransformDirection(Vector3.down);
        
        Vector3[] startPoint = StackTransforms()
            .Select(p => new Vector3(p.position.x, p.position.y - p.localScale.y / 2, p.position.z) - downward * 0.1f)
            .OrderBy(x => x.x)
            .ThenBy(x => x.z)
            .ToArray();

        for (int i = 0; i < 3; i++)
            if (Physics.Raycast(startPoint[i], downward, out RaycastHit hit, 10f))
                groundClearance[i] = Mathf.FloorToInt(hit.distance);
            else
            {
                Array.Fill(groundClearance, -1);
                break;
            }

        _groundClearance.Value = (int[])groundClearance.Clone();
    }
    #endregion

    public int[] GetGroundClearance() => _groundClearance.Value;

    public Func<Transform[]> StackTransforms { private get; set; }
}