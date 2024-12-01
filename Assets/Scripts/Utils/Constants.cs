using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

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
			public const string PIECE = "Prefabs/Piece.prefab";
		}

		/// <summary>
		/// Constants related to the game's gameplay mechanics.
		/// </summary>
		public static class Gameplay
		{
			public const int BOARD_ROWS = 6;
			public const int BOARD_COLUMNS = 4;
			public const int MAX_PIECE_SLICES = 6;
			public const int MAX_DECK_PIECES = 3;
			public const float PIECE_DESKTOP_SPEED = 1f;
			public const float PIECE_MOBILE_SPEED = 3f;
			public static readonly Vector2 PIECE_DESKTOP_OFFSET = new Vector2(0f, 0.2f);
			public static readonly Vector2 PIECE_MOBILE_OFFSET = new Vector2(0f, 1f); 
			public static readonly Color TILE_OVERING_COLOR = Color.clear;
		}
	}
}