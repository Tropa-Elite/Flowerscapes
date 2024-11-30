using System.Collections.Generic;
using UnityEngine;

namespace Game.Utils
{
	/// <summary>
	/// This class contains all the constants used throughout the game.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// Constants related to the game's scenes.
		/// </summary>
		public static class Scenes
		{
			public const string BOOT = "Boot";
			public const string MAIN = "Main";
		}

		/// <summary>
		/// Constants related to the game's prefabs.
		/// </summary>
		public static class Prefabs
		{
			public static string PIECE = "Prefabs/Piece.prefab";
		}

		/// <summary>
		/// Constants related to the game's gameplay mechanics.
		/// </summary>
		public static class Gameplay
		{
			public static int BOARD_ROWS = 6;
			public static int BOARD_COLUMNS = 4;
			public static int MAX_PIECE_SLICES = 6;
			public static int MAX_DECK_PIECES = 3;
		}
	}
}