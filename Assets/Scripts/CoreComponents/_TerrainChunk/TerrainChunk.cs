using System;
using System.Linq;
using UnityEngine;

public class TerrainChunk : CoreComponent, IPieceCollidable
{
    [Header("Terrain Data")]
    [Tooltip("Scriptable Object that defines the terrain heights.")]
    [SerializeField] private TerrainDataSO terrainData;

    [Header("Cube Appearance")]
    [SerializeField] private Material cubeMaterial;

    [SerializeField] private bool pieceCollisionEnabled = true;

    protected override (string name, Func<object> binding)[] Bindings => Array.Empty<(string, Func<object>)>();

    #region MonoBehavior
    protected override void Awake()
    {
        base.Awake();
        if (terrainData == null)
            Debug.LogError($"[TerrainChunk] Missing TerrainDataSO on {gameObject.name}.");
        else InterfaceRegistry<IPieceCollidable>.Register(this);

        for (int x = 0; x < terrainData.Length; x++) for (int z = 0; z < terrainData.Width; z++)
        {
            float cubeHeight = terrainData.GetHeightAt(x, z);

            // Create a built-in Unity cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Parent under this TerrainChunk GameObject
            cube.transform.parent = transform;

            // Scale and position
            cube.transform.localScale = new Vector3(1, cubeHeight, 1);
            cube.transform.localPosition = new Vector3(x, cubeHeight / 2f, z);

            // Assign the chosen material
            if (cubeMaterial != null && cube.TryGetComponent<Renderer>(out var renderer)) renderer.material = cubeMaterial;
        }
    }
    #endregion

    #region IPieceCollidable
    public bool IsCollidedWithPiece((int x, int z, int bottom)[] pieceBottoms){
        return terrainData != null && pieceCollisionEnabled &&
        pieceBottoms.Any(stack =>
            stack.bottom <
                terrainData.GetHeightAt(stack.x, stack.z)
        );
    }
    
    public void SetPieceCollisionEnabled(bool isEnabled) => pieceCollisionEnabled = isEnabled;
    #endregion
}