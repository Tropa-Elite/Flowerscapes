using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;
using System.Collections.Generic;

namespace Game.Logic
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the player's board during a gameplay session
	/// </summary>
	public interface IGameplayBoardDataProvider
	{
		IObservableDictionaryReader<UniqueId, IPieceData> Pieces { get; }

		bool TryGetPieceFromTile(int row, int column, out IPieceData pieceCopy);
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> Pieces { get; }

		bool TryGetPieceDataFromTile(int row, int column, out PieceData piece);

		void SetPieceOnTile(int row, int column, UniqueId id);

		void CleanUpTile(int row, int column);
	}

	/// <inheritdoc cref="IGameplayBoardLogic"/>
	public class GameplayBoardLogic : AbstractBaseLogic<PlayerData>, IGameplayBoardLogic, IGameLogicInitializer
	{
		private IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> _pieces;

		/// <inheritdoc />
		public IObservableDictionaryReader<UniqueId, IPieceData> Pieces => _pieces;

		/// <inheritdoc />
		IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> IGameplayBoardLogic.Pieces => _pieces;

		public GameplayBoardLogic(IConfigsProvider configsProvider, IDataProvider dataProvider, ITimeService timeService) :
			base(configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_pieces = new ObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData>(Data.Pieces,
				originPair => new KeyValuePair<UniqueId, IPieceData>(originPair.Key, originPair.Value),
				(key, value) => new KeyValuePair<UniqueId, PieceData>(key, value as PieceData));
		}

		/// <inheritdoc />
		public bool TryGetPieceFromTile(int row, int column, out IPieceData piece)
		{
			var ret = TryGetPieceDataFromTile(row, column, out var pieceData);

			piece = pieceData;

			return ret;
		}

		/// <inheritdoc />
		public bool TryGetPieceDataFromTile(int row, int column, out PieceData piece)
		{
			if (row < 0 || column < 0 || row >= Constants.Gameplay.BOARD_ROWS || column >= Constants.Gameplay.BOARD_COLUMNS)
			{
				piece = null;

				return false;
			}

			if (!Data.Pieces.TryGetValue(Data.Board[row, column].Piece, out var pieceData))
			{
				piece = null;

				return false;
			}

			piece = pieceData;

			return true;
		}

		/// <inheritdoc />
		public void SetPieceOnTile(int row, int column, UniqueId piece)
		{
			if (Data.Board[row, column] != null)
			{
				Data.Board[row, column].Piece = piece;

				return;
			}

			Data.Board[row, column] = new TileData
			{
				Row = row,
				Column = column,
				Piece = piece
			};
		}

		/// <inheritdoc />
		public void CleanUpTile(int row, int column)
		{
			_pieces.Remove(Data.Board[row, column].Piece);

			Data.Board[row, column].Piece = UniqueId.Invalid;
		}
	}
}
