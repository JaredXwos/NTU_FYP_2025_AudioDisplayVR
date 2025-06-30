using UnityEngine;

public interface ICanCollideWithPiece
{
    bool CollidedWithPiece((int x, int z, int bottom)[] pieceBottoms);
    void SetPieceCollisionEnabled(bool isEnabled);
}