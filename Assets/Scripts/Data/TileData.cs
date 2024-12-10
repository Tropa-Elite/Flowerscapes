using Game.Ids;
namespace Game.Data
{
	public interface ITileData
	{
		int Row { get; }
		int Column { get; }
		UniqueId PieceId { get; }
		
		int Id { get; }
	}

	public class TileData : ITileData
	{
		public int Row;
		public int Column;
		public UniqueId PieceId;

		int ITileData.Row => this.Row;
		int ITileData.Column => this.Column;
		UniqueId ITileData.PieceId => this.PieceId;
		public int Id => ToTileId(Row, Column);
		
		public static int ToTileId(int row, int column) => row * 100 + column;
		public static (int, int) IdToRowColumn(int id) => (id / 100, id % 100);
	}
}
