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
			    row >= Constants.Gameplay.BOARD_ROWS || column >= Constants.Gameplay.BOARD_COLUMNS ||
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
			var tileCount = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS;

			if(GameDataProvider.PieceDataProvider.Pieces.Count - _pieceDeck.Count < tileCount) return false;

			for (var i = 0; i < Constants.Gameplay.BOARD_ROWS; i++)
			{
				for (var j = 0; j < Constants.Gameplay.BOARD_COLUMNS; j++)
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
			var tile = Data.Board[row, column];
			var adjacentTiles = GetAdjacentTileList(row, column);
			var slices = pieceLogic.Pieces[tile.PieceId].GetSlicesColors();
			var slicesCache = new Dictionary<SliceColor, IPieceData>();
			var transferDone = false;
			var counter = 10;
			
			transferHistory = new List<PieceTransferData>();

			do
			{
				transferDone = false;

				foreach (var nextTile in adjacentTiles)
				{
					transferDone = TryTransferSlices(tile, nextTile, pieceLogic, slices, slicesCache, transferHistory) || transferDone;
				}
			} 
			while (transferDone && counter-- > 0);

			adjacentTiles.Insert(0, tile);

			foreach (var nextTile in adjacentTiles)
			{
				var piece = pieceLogic.Pieces[nextTile.PieceId];
				
				if (piece.Slices.Count == 0 || piece.Slices.Count == Constants.Gameplay.MAX_PIECE_SLICES)
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
					CleanUpTile(i, j);
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

		private bool TryTransferSlices(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, IPieceData> slicesCache,
			List<PieceTransferData> transferHistory)
		{
			var centerPiece = pieceLogic.Pieces[centerTile.PieceId];
			var nextPiece = pieceLogic.Pieces[nextTile.PieceId];
			
			if (nextPiece.IsFull || nextPiece.IsEmpty)
			{
				return false;
			}
			
			var nextSlices = nextPiece.GetSlicesColors();

			foreach (var slicePair in nextSlices)
			{
				var color = slicePair.Key;
				var canReceiveSlices = nextSlices.Count > 1 && centerPiece.SlicesFreeSpace > slicePair.Value;
				
				// Check first if the target piece has the color being processed to transfer or then give it up from cache collection
				if (centerSlices.ContainsKey(color))
				{
					if (!centerPiece.IsEmpty && !centerPiece.IsFull && (centerSlices.Count == 1 || canReceiveSlices))
					{
						TransferToCenterTile(centerTile, nextTile, pieceLogic, centerSlices, color, out var transferCount);
						transferHistory.Add(new PieceTransferData(nextTile.Id, centerTile.Id, nextPiece.Id, centerPiece.Id, color, transferCount));
						
						return true;
					}
					if (nextSlices.Count == 1)
					{
						TransferFromCenterTile(centerTile, nextTile, pieceLogic, centerSlices, slicesCache, color, out var transferCount);
						transferHistory.Add(new PieceTransferData(nextTile.Id, centerTile.Id, centerPiece.Id, nextPiece.Id, color, transferCount));
						
						return true;
					}
				}
				else if (TryTransferFromCache(centerTile, nextTile, pieceLogic, centerSlices, slicesCache, color))
				{
					transferHistory.Add(new PieceTransferData(nextTile.Id, centerTile.Id, centerPiece.Id, nextPiece.Id, color, centerSlices[color]));
					
					return true;
				}
			}

			return false;
		}

		private void TransferToCenterTile(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, SliceColor color, out int transferCount)
		{
			transferCount = pieceLogic.TransferSlices(nextTile.PieceId, centerTile.PieceId, color);
			centerSlices[color] += transferCount;
		}

		private void TransferFromCenterTile(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, IPieceData> slicesCache,
			SliceColor color, out int transferCount)
		{
			var nextPiece = pieceLogic.Pieces[nextTile.PieceId];
			
			transferCount = pieceLogic.TransferSlices(centerTile.PieceId, nextTile.PieceId, color);
			centerSlices[color] -= transferCount;

			if (centerSlices[color] == 0)
			{
				centerSlices.Remove(color);
			}
			if (!nextPiece.IsFull)
			{
				slicesCache.Add(color, nextPiece);
			}
		}

		private bool TryTransferFromCache(ITileData centerTile, ITileData nextTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, IPieceData> slicesCache, SliceColor color)
		{
			if (!slicesCache.TryGetValue(color, out var cachePiece) || cachePiece.Id == nextTile.PieceId || cachePiece.IsFull)
			{
				return false;
			}
			
			centerSlices[color] = pieceLogic.TransferSlices(centerTile.PieceId, nextTile.PieceId, color, cachePiece.SlicesFreeSpace);

			slicesCache.Remove(color);
			
			return true;
		}
	}
}
