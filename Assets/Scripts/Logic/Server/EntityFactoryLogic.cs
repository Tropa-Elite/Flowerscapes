using Game.Data;
using Game.Ids;
using Game.Utils;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Logic.Shared
{
	/// <summary>
	/// Provides the necessary behaviour to manage the creation of entities for the entire game logic
	/// </summary>
	public interface IEntityFactoryDataProvider
	{
		/// <summary>
		/// UniqueId of the last created entity
		/// </summary>
		UniqueId LastUniqueId { get; }
	}

	/// <inheritdoc />
	public interface IEntityFactoryLogic : IEntityFactoryDataProvider
	{
		PieceData CreatePiece();
	}

	/// <inheritdoc cref="IEntityFactoryLogic"/>
	public class EntityFactoryLogic : AbstractBaseLogic<PlayerData>, IEntityFactoryLogic
	{
		private IGameLogicLocator _gameLogic;
		private List<SliceColor> _randomColors;

		/// <inheritdoc />
		public UniqueId LastUniqueId => Data.UniqueIdCounter;

		public EntityFactoryLogic(
			IGameLogicLocator gamelogic,
			IConfigsProvider configsProvider,
			IDataProvider dataProvider,
			ITimeService timeService) :
			base(gamelogic, configsProvider, dataProvider, timeService)
		{
			_gameLogic = gamelogic;
			_randomColors = new List<SliceColor>();

			for(var i = 0; i < (int) SliceColor.ColorCount; i++)
			{
				_randomColors.Add((SliceColor)i);
			}
		}

		/// <inheritdoc />
		public PieceData CreatePiece()
		{
			var slicesCount = _gameLogic.RngLogic.Range(1, Constants.Gameplay.Max_Piece_Slices - 1);
			var colorCount = _gameLogic.RngLogic.Range(1, Math.Min(slicesCount, 3), true);
			var colors = _randomColors.OrderBy(_ => _gameLogic.RngLogic.Next).ToList();
			var minSlicesPerColor = (int)Math.Ceiling(slicesCount / (double)colorCount);
			var maxSlicesPerColor = slicesCount - (colorCount - 1);
			var slicesColorCounter = _gameLogic.RngLogic.Range(minSlicesPerColor, maxSlicesPerColor, true) - 1;
			var piece = new PieceData
			{
				Id = ++Data.UniqueIdCounter,
				Slices = new List<SliceColor>(slicesCount)
			};

			_randomColors.OrderBy(_ => _gameLogic.RngLogic.Next);

			for (int i = 0, colorIndex = 0; i < slicesCount; i++, slicesColorCounter--)
			{
				piece.Slices.Add(colors[colorIndex]);

				if (slicesColorCounter == 0 && i + 1 < slicesCount)
				{
					colorIndex += 1;
					minSlicesPerColor = (int)Math.Ceiling((slicesCount - i - 1f) / (colorCount - colorIndex));
					maxSlicesPerColor = slicesCount - i - 1 - (colorCount - colorIndex - 1);
					slicesColorCounter = _gameLogic.RngLogic.Range(minSlicesPerColor, maxSlicesPerColor, true);
				}
			}

			_gameLogic.PiecesLogic.Pieces.Add(piece.Id, piece);

			return piece;
		}
	}
}
