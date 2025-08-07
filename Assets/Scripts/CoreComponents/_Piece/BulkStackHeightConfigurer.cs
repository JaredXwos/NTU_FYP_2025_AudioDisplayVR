using UnityEngine;

[System.Serializable]
public struct PieceConfiguration
{
    public Piece Piece;
    public Vector3Int Values;
    public float Weight;
    public PieceConfiguration(Piece piece, Vector3Int values, float weight)
    {
        Values = values;
        Piece = piece;
        Weight = weight;
    }
}

public class BulkStackHeightConfigurer : Admin
{
    [SerializeField] private PieceConfiguration[] Configurations;

    private void Start()
    {
        foreach(PieceConfiguration config in Configurations)
            if(config.Piece != null)
            {
                ((ILimitedAccess)config.Piece).Authenticate();
                config.Piece.GetComponent<ConstantWeight>()?.SetWeight(config.Weight, Key);
                config.Piece.ResetHeights(config.Values);
            }
    }
}