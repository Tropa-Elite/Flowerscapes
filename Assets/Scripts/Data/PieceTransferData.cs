namespace Game.Data
{
    public struct PieceTransferData
    {
        public int OriginTileId;
        public int TargetTileId;
        public SliceColor Slices;
        public int SlicesAmount;
        
        public PieceTransferData(int originTileId, int targetTileId, 
            SliceColor slices, int slicesAmount)
        {
            OriginTileId = originTileId;
            TargetTileId = targetTileId;
            Slices = slices;
            SlicesAmount = slicesAmount;
        }
    }
}