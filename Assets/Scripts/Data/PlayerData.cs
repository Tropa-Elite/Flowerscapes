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

		public Dictionary<ulong, PieceData> Pieces = new Dictionary<ulong, PieceData>();
		public List<UniqueId> PieceDeck = new List<UniqueId>(Constants.Gameplay.MAX_DECK_PIECES);
		public TileData[,] Board = new TileData[Constants.Gameplay.BOARD_ROWS, Constants.Gameplay.BOARD_COLUMNS];
	}
}