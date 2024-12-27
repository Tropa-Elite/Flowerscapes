using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Logic.Client
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the player's board during a gameplay session
	/// </summary>
	public interface IGameplayBoardDataProvider
	{
		IObservableListReader<UniqueId> PieceDeck { get; }

		bool TryGetTileData(int row, int column, out ITileData tile);

		bool TryGetPieceFromTile(int row, int column, out IPieceData piece);

		bool IsGameOver();

		List<ITileData> GetAdjacentTileList(int row, int column);
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableList<UniqueId> PieceDeck { get; }

		void SetPieceOnTile(UniqueId pieceId, int row, int column);

		void ActivateTile(int row, int column, IPiecesLogic pieceLogic, out List<PieceTransferData> transferHistory);

		void CleanUpTile(int row, int column);

		void RefillPieceDeck(Func<PieceData> createPieceFunc);

		void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic);
	}

	/// <inheritdoc cref="IGameplayBoardLogic"/>
	public class GameplayBoardLogic : AbstractBaseLogic<PlayerData>, IGameplayBoardLogic, IGameLogicInitializer
	{
		private IObservableList<UniqueId> _pieceDeck;

		/// <inheritdoc />
		public IObservableList<UniqueId> PieceDeck => _pieceDeck;
		/// <inheritdoc />
		IObservableListReader<UniqueId> IGameplayBoardDataProvider.PieceDeck => _pieceDeck;

		public GameplayBoardLogic(
			IGameDataProviderLocator gameDataProvider, 
			IConfigsProvider configsProvider, 
			IDataProvider dataProvider, 
			ITimeService timeService) :
			base(gameDataProvider, configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_pieceDeck = new ObservableList<UniqueId>(Data.PieceDeck);
		}

		/// <inheritdoc />
		public bool TryGetTileData(int row, int column, out ITileData tile)
		{
			tile = null;
			
			if (row < 0 || column < 0 || 
			    row >= Constants.Gameplay.Board_Rows || column >= Constants.Gameplay.Board_Columns ||
			    Data.Board[row, column] == null)
			{
				return false;
			}
			
			tile = Data.Board[row, column];

			return true;
		}

		/// <inheritdoc />
		public bool TryGetPieceFromTile(int row, int column, out IPieceData piece)
		{
			piece = null;
			
			if (!TryGetTileData(row, column, out var tile) || 
			    !GameDataProvider.PieceDataProvider.Pieces.TryGetValue(tile.PieceId, out piece))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public bool IsGameOver()
		{
			var tileCount = Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns;

			if(GameDataProvider.PieceDataProvider.Pieces.Count - _pieceDeck.Count < tileCount) return false;

			for (var i = 0; i < Constants.Gameplay.Board_Rows; i++)
			{
				for (var j = 0; j < Constants.Gameplay.Board_Columns; j++)
				{
					if (Data.Board[i, j] == null || !Data.Board[i, j].PieceId.IsValid)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc />
		public List<ITileData> GetAdjacentTileList(int row, int column)
		{
			var list = new List<ITileData>();

			for (var i = -1; i < 2; i++)
			{
				if (i != 0 && TryGetTileData(row + i, column, out var tile) && tile.PieceId.IsValid)
				{
					list.Add(tile);
				}
			}

			for (var i = -1; i < 2; i++)
			{
				if (i != 0 && TryGetTileData(row, column + i, out var tile) && tile.PieceId.IsValid)
				{
					list.Add(tile);
				}
			}

			return list;
		}

		/// <inheritdoc />
		public void SetPieceOnTile(UniqueId pieceId, int row, int column)
		{
			if (Data.Board[row, column] != null)
			{
				Data.Board[row, column].PieceId = pieceId;

				return;
			}

			Data.Board[row, column] = new TileData
			{
				Row = row,
				Column = column,
				PieceId = pieceId
			};
		}

		/// <inheritdoc />
		public void ActivateTile(int row, int column, IPiecesLogic pieceLogic, out List<PieceTransferData> transferHistory)
		{
			var centerTile = Data.Board[row, column];
			var adjacentTiles = GetAdjacentTileList(row, column);
			var slicesCache = new Dictionary<SliceColor, IPieceData>();
			var transferDone = false;
			
			transferHistory = new List<PieceTransferData>();

			do
			{
				transferDone = false;

				foreach (var nextTile in adjacentTiles)
				{
					transferDone = TryTransferSlices(centerTile, nextTile, pieceLogic, slicesCache, transferHistory) || transferDone;
				}
			} 
			while (transferDone);

			adjacentTiles.Insert(0, centerTile);

			foreach (var nextTile in adjacentTiles)
			{
				var piece = pieceLogic.Pieces[nextTile.PieceId];
				
				if (piece.Slices.Count == 0 || piece.Slices.Count == Constants.Gameplay.Max_Piece_Slices)
				{
					pieceLogic.Pieces.Remove(nextTile.PieceId);
					CleanUpTile(nextTile.Row, nextTile.Column);
				}
			}
		}

		/// <inheritdoc />
		public void CleanUpTile(int row, int column)
		{
			if(Data.Board[row, column] != null)
			{
				Data.Board[row, column].PieceId = UniqueId.Invalid;
			}
		}

		/// <inheritdoc />
		public void RefillPieceDeck(Func<PieceData> createPieceFunc)
		{
			PieceDeck.Clear();

			for (var i = 0; i < Constants.Gameplay.Max_Deck_Pieces; i++)
			{
				PieceDeck.Add(createPieceFunc().Id);
			}
		}

		/// <inheritdoc />
		public void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic)
		{
			for (var i = 0; i < Constants.Gameplay.Board_Rows; i++)
			{
				for (var j = 0; j < Constants.Gameplay.Board_Columns; j++)
				{
					CleanUpTile(i, j);
				}
			}

			var totalSpace = Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns;
			var totalPieces = rngLogic.Range(totalSpace / 4, totalSpace / 2);

			for (int i = 0, pos = -1; i < totalPieces; i++)
			{
				pos = rngLogic.Range(pos + 1, totalSpace - totalPieces + i);

				SetPieceOnTile(createPieceFunc().Id,
					pos / Constants.Gameplay.Board_Columns,
					pos % Constants.Gameplay.Board_Columns);
			}
		}

		private bool TryTransferSlices(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			 Dictionary<SliceColor, IPieceData> slicesCache, List<PieceTransferData> transferHistory)
		{
			var centerPiece = pieceLogic.Pieces[centerTile.PieceId];
			var nextPiece = pieceLogic.Pieces[nextTile.PieceId];
			
			if (nextPiece.IsFull || nextPiece.IsEmpty)
			{
				return false;
			}
			
			var centerSlices = centerPiece.GetSlicesColors();
			var nextSlices = nextPiece.GetSlicesColors();

			foreach (var slicePair in nextSlices)
			{
				if (TryTransferToCenter(centerTile, nextTile, pieceLogic, centerSlices, nextSlices, slicePair, out var transfer) ||
				    TryTransferFromCenter(centerTile, nextTile, pieceLogic, centerSlices, nextSlices, slicesCache, slicePair.Key, out transfer) ||
				    TryTransferToCenterFromCache(centerTile, nextTile, pieceLogic, slicesCache, slicePair.Key, out transfer))
				{
					transferHistory.Add(transfer);
					
					return true;
				}
			}

			return false;
		}

		private bool TryTransferToCenter(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, int> nextSlices, 
			KeyValuePair<SliceColor, int> color, out PieceTransferData transfer)
		{
			var centerPiece = pieceLogic.Pieces[centerTile.PieceId];
			// Check the 11 | 1133  -> 1111 | 33 or 1122 | 1133 -> 111122 | 33 cases where center can accept extra slices
			var canAcceptExtraSlices = nextSlices.Count > 1 && centerPiece.SlicesFreeSpace >= color.Value;
			var canAcceptSlices = centerSlices.Count == 1 || canAcceptExtraSlices;
			
			if(!centerSlices.ContainsKey(color.Key) || centerPiece.IsEmpty || centerPiece.IsFull || !canAcceptSlices)
			{
				transfer = default;
				
				return false;
			}
			
			var amount = pieceLogic.TransferSlices(nextTile.PieceId, centerTile.PieceId, color.Key);
			
			transfer = new PieceTransferData(nextTile.Id, centerTile.Id, nextTile.PieceId, centerTile.PieceId, color.Key, amount);

			return true;
		}

		private bool TryTransferFromCenter(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, int> nextSlices, 
			Dictionary<SliceColor, IPieceData> slicesCache, SliceColor color, out PieceTransferData transfer)
		{
			var centerPiece = pieceLogic.Pieces[centerTile.PieceId];
			var nextPiece = pieceLogic.Pieces[nextTile.PieceId];
			
			if(!centerSlices.ContainsKey(color) || centerPiece.IsComplete || nextSlices.Count > 1)
			{
				transfer = default;

				return false;
			}
			
			var amount = pieceLogic.TransferSlices(centerTile.PieceId, nextTile.PieceId, color);
			
			transfer = new PieceTransferData(centerTile.Id, nextTile.Id, centerTile.PieceId, nextPiece.Id, color, amount);
			
			if (!nextPiece.IsFull)
			{
				slicesCache.Add(color, nextPiece);
			}

			return true;
		}

		private bool TryTransferToCenterFromCache(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic, 
			Dictionary<SliceColor, IPieceData> slicesCache, SliceColor color, out PieceTransferData transfer)
		{
			var centerPiece = pieceLogic.Pieces[centerTile.PieceId];
			
			if(!slicesCache.TryGetValue(color, out var cachePiece) || cachePiece.Id == nextTile.PieceId || cachePiece.IsFull)
			{
				transfer = default;

				return false;
			}
			
			var centerColorAmount = centerPiece.GetSlicesCount(color);
			var maxSlices = cachePiece.SlicesFreeSpace - centerColorAmount;
			var amount = pieceLogic.TransferSlices(nextTile.PieceId, centerTile.PieceId, color, maxSlices);
			
			transfer = new PieceTransferData(nextTile.Id, centerTile.Id, nextTile.PieceId, centerTile.PieceId, color, amount);

			if (centerPiece.IsFull || cachePiece.SlicesFreeSpace == centerColorAmount + amount)
			{
				slicesCache.Remove(color);
			}

			return true;
		}
	}
}
