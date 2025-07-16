public interface IPieceCollidable
{
    public bool IsCollidedWithPiece((int x, int z, int bottom)[] pieceBottoms);
    public void SetPieceCollisionEnabled(bool isEnabled);
}