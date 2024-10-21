using Game.Ids;
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
	}
}
