using System;
using UnityEngine;

public class TerrainChunk : CollidingComponent
{
    [Header("Terrain Data")]
    [Tooltip("Scriptable Object that defines the terrain heights.")]
    [SerializeField] private TerrainDataSO data;

    protected override void Awake()
    {
        base.Awake();
        if (data == null)
            Debug.LogError($"[TerrainChunk_] Missing TerrainDataSO on {gameObject.name}.");

        targetBody.Clear();
        for (int x = 0; x < data.Length; x++)
            for (int z = 0; z < data.Width; z++)
                for (int y = 0; y < Mathf.RoundToInt(data.GetHeightAt(x, z)); y++)
                    targetBody.Add(new Vector3Int(x, y, z) + Vector3Int.RoundToInt(transform.position));
                
        World.TryRegister(targetBody, this);
        AttemptUpdate();
    }
    protected virtual void Update() => Render();

    protected override (string name, Func<object> binding)[] Bindings => new (string name, Func<object> binding)[0];
}