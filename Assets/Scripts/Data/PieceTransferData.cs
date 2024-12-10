using Game.Ids;

namespace Game.Data
{
    public struct PieceTransferData
    {
        public readonly int OriginTileId;
        public readonly int TargetTileId;
        public readonly UniqueId OriginPieceId;
        public readonly UniqueId TargetPieceId;
        public readonly SliceColor SliceColor;
        public readonly int SlicesAmount;
        
        public PieceTransferData(int originTileId, int targetTileId, UniqueId originPieceId, UniqueId targetPieceId,
            SliceColor color, int slicesAmount)
        {
            OriginTileId = originTileId;
            TargetTileId = targetTileId;
            OriginPieceId = originPieceId;
            TargetPieceId = targetPieceId;
            SliceColor = color;
            SlicesAmount = slicesAmount;
        }
    }
}