using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;
using System.Collections.Generic;

namespace Game.Logic.Client
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

		List<KeyValuePair<int, int>> GetPiecesNodeList(int row, int column);
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> Pieces { get; }
		new IObservableList<UniqueId> PieceDeck { get; }

		bool TryGetPieceDataFromTile(int row, int column, out PieceData piece);

		void SetPieceOnTile(UniqueId pieceId, int row, int column);

		void ActivateTile(int row, int column);

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
		public List<KeyValuePair<int, int>> GetPiecesNodeList(int row, int column)
		{
			var list = new List<KeyValuePair<int, int>>();

			if (TryGetPieceFromTile(row, row, out _))
			{
				list.Add(new KeyValuePair<int, int>(row, column));
			}

			for (var i = -1; i < 2; i++)
			{
				if (i == 0) continue;
				if (TryGetPieceFromTile(row + i, column, out _))
				{
					list.Add(new KeyValuePair<int, int>(row + i, column));
				}
			}

			for (var i = -1; i < 2; i++)
			{
				if (i == 0) continue;
				if (TryGetPieceFromTile(row, column + i, out _))
				{
					list.Add(new KeyValuePair<int, int>(row, column + i));
				}
			}

			return list;
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
		public void ActivateTile(int row, int column)
		{
			var piece = _pieces[Data.Board[row, column].Piece];
			var nodeList = GetPiecesNodeList(row, column);

			if(ColorCount(piece) != 1)
			{
				return;
			}

			var color = piece.Slices[0];
			var slicesCount = piece.Slices.Count;

			for (int i = 0; i < nodeList.Count && slicesCount < Constants.Gameplay.MAX_PIECE_SLICES; i++)
			{
				if (nodeList[i].Key == row && nodeList[i].Value == column) continue;

				var maxSlices = Constants.Gameplay.MAX_PIECE_SLICES - slicesCount;
				var addSlices = CollectSlicesFromTile(nodeList[i].Key, nodeList[i].Value, color, maxSlices);

				FillPiece(piece.Id, color, addSlices);

				slicesCount += addSlices;
			}

			// Piece is full
			if(slicesCount == Constants.Gameplay.MAX_PIECE_SLICES)
			{
				CleanUpTile(row, column);
			}
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
					if (TryGetPieceDataFromTile(i, j, out _))
					{
						CleanUpTile(i, j);
					}
				}
			}

			var totalSpace = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS;
			var totalPieces = rngLogic.Range(totalSpace / 4, totalSpace / 2);

			for (int i = 0, pos = -1; i < totalPieces; i++)
			{
				pos = rngLogic.Range(pos + 1, totalSpace - totalPieces + i);

				SetPieceOnTile(createPieceFunc().Id,
					pos / Constants.Gameplay.BOARD_COLUMNS,
					pos % Constants.Gameplay.BOARD_COLUMNS);
			}
		}

		private int ColorCount(IPieceData piece)
		{
			var count = 0;

			for (int i = 0; i < piece.Slices.Count; i++)
			{
				if (i == 0 || piece.Slices[i] != piece.Slices[i - 1])
				{
					count++;
				}
			}

			return count;
		}

		private int CollectSlicesFromTile(int row, int column, SliceColor color, int maxSlices)
		{
			var count = 0;

			if(TryGetPieceDataFromTile(row, column, out var piece))
			{
				for (int i = piece.Slices.Count - 1; i > -1 && count < maxSlices; i--)
				{
					if (piece.Slices[i] != color) continue;

					count++;

					piece.Slices.RemoveAt(i);
				}

				// Piece is empty
				if (piece.Slices.Count == 0)
				{
					CleanUpTile(row, column);
				}
			}

			return count;
		}

		private void FillPiece(UniqueId id, SliceColor color, int amount)
		{
			var piece = _pieces.GetOriginValue(id);
			var index = piece.Slices.IndexOf(color);

			for (int i = 0; i < amount; i++)
			{
				piece.Slices.Insert(index, color);
			}
		}
	}
}
