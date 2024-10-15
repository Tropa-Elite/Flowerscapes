using Game.Ids;
using System.Collections.Generic;

namespace Game.Data
{
	public enum SliceColor
	{
		ENPTY,
		WHITE,
		BLACK,
		RED,
		ORANGE,
		YELLOW,
		GREEN,
		BLUE,
		VIOLET,
	}

	public interface IPieceData
	{
		UniqueId Id { get; }
		IReadOnlyList<SliceColor> Slices { get; }
	}

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
