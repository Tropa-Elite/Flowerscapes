using Game.Ids;
using Game.Utils;
using System.Collections.Generic;

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

		List<SliceColor> GetColors();
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
		public List<SliceColor> GetColors()
		{
			var list = new List<SliceColor>();

			for (int i = 0; i < Slices.Count; i++)
			{
				if (i == 0 || Slices[i] != Slices[i - 1])
				{
					list.Add(Slices[i]);
				}
			}

			return list;
		}

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
