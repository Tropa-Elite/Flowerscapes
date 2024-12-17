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
			public const string Boot = "Boot";
			public const string Main = "Main";
		}

		/// <summary>
		/// Constants related to the game's prefabs.
		/// </summary>
		public static class Prefabs
		{
			public const string Piece_Prefab = "Prefabs/Piece.prefab";
		}

		/// <summary>
		/// Constants related to the game's gameplay mechanics.
		/// </summary>
		public static class Gameplay
		{
			public const int Board_Rows = 6;
			public const int Board_Columns = 4;
			public const int Max_Piece_Slices = 6;
			public const int Max_Deck_Pieces = 3;
			public const float Piece_Desktop_Speed = 1f;
			public const float Piece_Mobile_Speed = 3f;
			public const float Piece_Pivot_Tween_Time = 0.4f;
			public const float Slice_Rotation_Delay_Time = 0.1f;
			public const float Slice_Rotation_Tween_Time = 0.2f;
			public const float Slice_Transfer_Delay_Time = 0.2f;
			public const float Slice_Transfer_Tween_Time = 0.4f;
			public const float Piece_Complete_Delay_Time = 0.1f;
			public const float Piece_Complete_Tween_Time = 0.4f;
			public static readonly Vector3 Slice_Rotation = new Vector3(0, 0, 60);
			public static readonly Vector2 Piece_Mobile_Offset = new Vector2(0f, 1f); 
			public static readonly Vector2 Piece_Desktop_Offset = new Vector2(0f, 0.2f);
			public static readonly Color Tile_Overing_Color = Color.clear;
		}
	}
}