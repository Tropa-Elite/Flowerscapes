using Game.Ids;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Game.Data
{
	public enum SliceColor
	{
		White,
		Black,
		Red,
		//Orange,
		Yellow,
		Green,
		Blue,
		//Violet,
		ColorCount					// The total number of different possible colors
	}

	public interface IPieceData
	{
		UniqueId Id { get; }
		IReadOnlyList<SliceColor> Slices { get; }
		int SlicesFreeSpace => Constants.Gameplay.MAX_PIECE_SLICES - Slices.Count;
		bool IsFull => Slices.Count == Constants.Gameplay.MAX_PIECE_SLICES;
		bool IsEmpty => Slices.Count == 0;
		bool IsComplete => IsFull && Slices.All(s => s == Slices[0]);

		Dictionary<SliceColor, int> GetSlicesColors();
	}

	/// <inheritdoc />
	public class PieceData : IPieceData
	{
		public UniqueId Id;
		public List<SliceColor> Slices = new List<SliceColor>();

		/// <inheritdoc />
		UniqueId IPieceData.Id => this.Id;

		/// <inheritdoc />
		IReadOnlyList<SliceColor> IPieceData.Slices => Slices;

		/// <inheritdoc />
		public Dictionary<SliceColor, int> GetSlicesColors()
		{
			var dictionary = new Dictionary<SliceColor, int>();

			foreach (var slice in Slices)
			{
				dictionary.TryGetValue(slice, out var count);

				dictionary[slice] = count + 1;
			}

			return dictionary;
		}
	}
}
