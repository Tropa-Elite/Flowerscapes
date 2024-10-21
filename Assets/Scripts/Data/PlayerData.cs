using System;
using System.Collections.Generic;
using Game.Ids;
using Game.Utils;

namespace Game.Data
{
	/// <summary>
	/// Contains all the data in the scope of the Player 
	/// </summary>
	[Serializable]
	public class PlayerData
	{
		public ulong UniqueIdCounter;

		public Dictionary<GameId, int> Currencies = new Dictionary<GameId, int>(new GameIdLookup.GameIdComparer())
		{
			{ GameId.SoftCurrency, 100 },
			{ GameId.HardCurrency, 10 }
		};

		public Dictionary<UniqueId, PieceData> Pieces = new Dictionary<UniqueId, PieceData>();
		public List<UniqueId> InputPieces = new List<UniqueId>(Constants.Gameplay.MAX_INPUT_PIECES);
		public TileData[,] Board = new TileData[Constants.Gameplay.BOARD_ROWS, Constants.Gameplay.BOARD_COLUMNS];
	}
}