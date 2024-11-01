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
		IObservableListReader<UniqueId> PieceDeck { get; }

		bool TryGetPieceFromTile(int row, int column, out IPieceData pieceCopy);

		bool IsGameOver();
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> Pieces { get; }
		new IObservableList<UniqueId> PieceDeck { get; }

		bool TryGetPieceDataFromTile(int row, int column, out PieceData piece);

		void SetPieceOnTile(UniqueId pieceId, int row, int column);

		void CleanUpTile(int row, int column);

		void RefillPieceDeck(Func<PieceData> createPieceFunc);

		void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic);
	}

	/// <inheritdoc cref="IGameplayBoardLogic"/>
	public class GameplayBoardLogic : AbstractBaseLogic<PlayerData>, IGameplayBoardLogic, IGameLogicInitializer
	{
		private IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> _pieces;
		private IObservableList<UniqueId> _pieceDeck;

		/// <inheritdoc />
		public IObservableDictionaryReader<UniqueId, IPieceData> Pieces => _pieces;
		/// <inheritdoc />
		IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> IGameplayBoardLogic.Pieces => _pieces;
		/// <inheritdoc />
		public IObservableList<UniqueId> PieceDeck => _pieceDeck;
		/// <inheritdoc />
		IObservableListReader<UniqueId> IGameplayBoardDataProvider.PieceDeck => _pieceDeck;

		public GameplayBoardLogic(IConfigsProvider configsProvider, IDataProvider dataProvider, ITimeService timeService) :
			base(configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_pieceDeck = new ObservableList<UniqueId>(Data.PieceDeck);
			_pieces = new ObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData>(Data.Pieces,
				originPair => new KeyValuePair<UniqueId, IPieceData>(originPair.Key, originPair.Value),
				(key, value) => new KeyValuePair<ulong, PieceData>(key, value as PieceData));
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

			if (Data.Board[row, column] == null || !_pieces.TryGetOriginValue(Data.Board[row, column].Piece, out var pieceData))
			{
				piece = null;

				return false;
			}

			piece = pieceData;

			return true;
		}

		/// <inheritdoc />
		public bool IsGameOver()
		{
			var tileCount = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS;

			if(_pieces.Count - _pieceDeck.Count < tileCount) return false;

			for (var i = 0; i < Constants.Gameplay.BOARD_ROWS; i++)
			{
				for (var j = 0; j < Constants.Gameplay.BOARD_COLUMNS; j++)
				{
					if (Data.Board[i, j] == null || !Data.Board[i, j].Piece.IsValid)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc />
		public void SetPieceOnTile(UniqueId pieceId, int row, int column)
		{
			if (Data.Board[row, column] != null)
			{
				Data.Board[row, column].Piece = pieceId;

				return;
			}

			Data.Board[row, column] = new TileData
			{
				Row = row,
				Column = column,
				Piece = pieceId
			};
		}

		/// <inheritdoc />
		public void CleanUpTile(int row, int column)
		{
			_pieces.Remove(Data.Board[row, column].Piece);

			Data.Board[row, column].Piece = UniqueId.Invalid;
		}

		/// <inheritdoc />
		public void RefillPieceDeck(Func<PieceData> createPieceFunc)
		{
			foreach (var id in PieceDeck)
			{
				_pieces.Remove(id);
			}

			PieceDeck.Clear();

			for (var i = 0; i < Constants.Gameplay.MAX_DECK_PIECES; i++)
			{
				PieceDeck.Add(createPieceFunc().Id);
			}
		}

		/// <inheritdoc />
		public void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic)
		{
			for (var i = 0; i < Constants.Gameplay.BOARD_ROWS; i++)
			{
				for (var j = 0; j < Constants.Gameplay.BOARD_COLUMNS; j++)
				{
					if (TryGetPieceDataFromTile(i, j, out var piece))
					{
						CleanUpTile(i, j);
					}
				}
			}

			var totalSpace = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS;
			var totalPieces = rngLogic.Range(totalSpace / 4, totalSpace / 2);

			for (int i = 0, pos = 0; i < totalPieces; i++)
			{
				pos = rngLogic.Range(pos, totalSpace - totalPieces + i);

				SetPieceOnTile(createPieceFunc().Id,
					pos / Constants.Gameplay.BOARD_COLUMNS,
					pos % Constants.Gameplay.BOARD_COLUMNS);
			}
		}
	}
}
