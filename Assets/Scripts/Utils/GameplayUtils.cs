using Game.Data;
using System;

namespace Game.Utils
{
	public static class GameplayUtils
	{
		public static int CovertToId(this TileData tile)
		{
			return CovertTileToId(tile.Row, tile.Column);
		}

		public static int CovertTileToId(int row, int column)
		{
			var columnSize = Constants.Gameplay.BOARD_COLUMNS.ToString().Length;

			return row * (int) Math.Pow(10, columnSize) + column;
		}
	}
}
