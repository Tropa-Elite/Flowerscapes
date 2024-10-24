using Game.Data;
using Game.Ids;
using Game.Utils;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using System.Collections.Generic;

namespace Game.Logic.Shared
{
	public interface IEntityFactoryDataProvider
	{
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
		private IGameLogic _gameLogic;

		/// <inheritdoc />
		public UniqueId LastUniqueId => Data.UniqueIdCounter;

		public EntityFactoryLogic(
			IGameLogic gamelogic, 
			IConfigsProvider configsProvider, 
			IDataProvider dataProvider, 
			ITimeService timeService) :
			base(configsProvider, dataProvider, timeService)
		{
			_gameLogic = gamelogic;
		}

		/// <inheritdoc />
		public PieceData CreatePiece()
		{
			var slicesCount = _gameLogic.RngLogic.Range(1, Constants.Gameplay.MAX_PIECE_SLICES - 1);
			var colorCount = _gameLogic.RngLogic.Range(1, slicesCount);
			var lastColor = (SliceColor)_gameLogic.RngLogic.Range(0, (int)SliceColor.ColorCount);
			var piece = new PieceData
			{
				Id = ++Data.UniqueIdCounter,
				Slices = new List<SliceColor>(slicesCount)
			};

			for (var i = 0; i < slicesCount; i++)
			{
				piece.Slices.Add(lastColor);

				if (slicesCount - i == colorCount)
				{
					colorCount--;
					lastColor = (SliceColor)_gameLogic.RngLogic.Range(0, (int)SliceColor.ColorCount);
				}
			}

			_gameLogic.GameplayBoardLogic.Pieces.Add(piece.Id, piece);

			return piece;
		}
	}
}
