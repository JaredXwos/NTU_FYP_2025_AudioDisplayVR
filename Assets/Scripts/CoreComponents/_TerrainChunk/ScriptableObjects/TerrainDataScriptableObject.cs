using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainData", menuName = "Terrain/TerrainDataSO")]
public class TerrainDataSO : ScriptableObject
{
    [SerializeField] private int width;
    [SerializeField] private int length;
    [SerializeField] private List<float> heights = new();

    public int Width => width;
    public int Length => length;
    public IReadOnlyList<float> Heights => heights;

    private void OnEnable() => ValidateSize();

    public void ValidateSize()
    {
        int expected = length * width;
        if (heights.Count != expected)
        {
            Debug.LogWarning($"[TerrainDataSO] Adjusting heights from {heights.Count} to {expected} ({length}x{width})");
            while (heights.Count < expected)
                heights.Add(0f);
        }
    }

    public float GetHeightAt(int x, int z) => (x < 0 || x >= length || z < 0 || z >= width)? 0f : heights[z * length + x];
}