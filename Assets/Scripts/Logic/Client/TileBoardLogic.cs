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
	/// This logic provides the necessary behaviour to manage the player's level tile board
	/// </summary>
	public interface ITileBoardDataProvider
	{
		bool TryGetTileData(int row, int column, out ITileData tile);

		bool TryGetPieceFromTile(int row, int column, out IPieceData piece);

		List<ITileData> GetAdjacentTileList(int row, int column);
	}

	/// <inheritdoc />
	public interface ITileBoardLogic : ITileBoardDataProvider
	{
		void SetPieceOnTile(UniqueId pieceId, int row, int column);

		void ActivateTile(int row, int column, out List<ITileData> tiles, out List<PieceTransferData> transfers);

		void CleanUpTile(int row, int column);

		void RefillBoard();
	}

	/// <inheritdoc cref="ITileBoardLogic"/>
	public class TileBoardLogic : AbstractBaseLogic<PlayerData>, ITileBoardLogic
	{
		public TileBoardLogic(
			IGameLogicLocator gameLogic, 
			IConfigsProvider configsProvider, 
			IDataProvider dataProvider, 
			ITimeService timeService) :
			base(gameLogic, configsProvider, dataProvider, timeService)
		{
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
			    !GameLogic.PiecesLogic.Pieces.TryGetValue(tile.PieceId, out piece))
			{
				return false;
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
		public void ActivateTile(int row, int column, out List<ITileData> tiles, out List<PieceTransferData> transfers)
		{
			var centerTile = Data.Board[row, column];
			var slicesCache = new Dictionary<SliceColor, IPieceData>();
			var transferDone = false;
			
			tiles = GetAdjacentTileList(row, column);
			transfers = new List<PieceTransferData>();

			do
			{
				transferDone = false;

				foreach (var nextTile in tiles)
				{
					transferDone = TryTransferSlices(centerTile, nextTile, slicesCache, transfers) || transferDone;
				}
			} 
			while (transferDone);

			tiles.Insert(0, centerTile);
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
		public void RefillBoard()
		{
			for (var i = 0; i < Constants.Gameplay.Board_Rows; i++)
			{
				for (var j = 0; j < Constants.Gameplay.Board_Columns; j++)
				{
					CleanUpTile(i, j);
				}
			}

			var totalSpace = Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns;
			var totalPieces = GameLogic.RngLogic.Range(totalSpace / 4, totalSpace / 2);

			for (int i = 0, pos = -1; i < totalPieces; i++)
			{
				pos = GameLogic.RngLogic.Range(pos + 1, totalSpace - totalPieces + i);

				SetPieceOnTile(GameLogic.EntityFactoryLogic.CreatePiece().Id,
					pos / Constants.Gameplay.Board_Columns,
					pos % Constants.Gameplay.Board_Columns);
			}
		}

		private bool TryTransferSlices(ITileData centerTile, ITileData nextTile, 
			Dictionary<SliceColor, IPieceData> slicesCache, List<PieceTransferData> transfers)
		{
			var centerPiece = GameLogic.PiecesLogic.Pieces[centerTile.PieceId];
			var nextPiece = GameLogic.PiecesLogic.Pieces[nextTile.PieceId];
			
			if (nextPiece.IsFull || nextPiece.IsEmpty)
			{
				return false;
			}
			
			var centerSlices = centerPiece.GetSlicesColors();
			var nextSlices = nextPiece.GetSlicesColors();

			foreach (var slicePair in nextSlices)
			{
				if (TryTransferToCenter(centerTile, nextTile, centerSlices, nextSlices, slicePair, out var transfer) ||
				    TryTransferFromCenter(centerTile, nextTile, centerSlices, nextSlices, slicesCache, slicePair.Key, out transfer) ||
				    TryTransferToCenterFromCache(centerTile, nextTile, slicesCache, slicePair.Key, out transfer))
				{
					transfers.Add(transfer);
					
					return true;
				}
			}

			return false;
		}

		private bool TryTransferToCenter(ITileData centerTile, ITileData nextTile, 
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, int> nextSlices, 
			KeyValuePair<SliceColor, int> color, out PieceTransferData transfer)
		{
			var centerPiece = GameLogic.PiecesLogic.Pieces[centerTile.PieceId];
			// Check the 11 | 1133  -> 1111 | 33 or 1122 | 1133 -> 111122 | 33 cases where center can accept extra slices
			var canAcceptExtraSlices = nextSlices.Count > 1 && centerPiece.SlicesFreeSpace >= color.Value;
			var canAcceptSlices = centerSlices.Count == 1 || canAcceptExtraSlices;
			
			if(!centerSlices.ContainsKey(color.Key) || centerPiece.IsEmpty || centerPiece.IsFull || !canAcceptSlices)
			{
				transfer = default;
				
				return false;
			}
			
			var amount = GameLogic.PiecesLogic.TransferSlices(nextTile.PieceId, centerTile.PieceId, color.Key);
			
			transfer = new PieceTransferData(nextTile.Id, centerTile.Id, nextTile.PieceId, centerTile.PieceId, color.Key, amount);

			return true;
		}

		private bool TryTransferFromCenter(ITileData centerTile, ITileData nextTile, 
			Dictionary<SliceColor, int> centerSlices, Dictionary<SliceColor, int> nextSlices, 
			Dictionary<SliceColor, IPieceData> slicesCache, SliceColor color, out PieceTransferData transfer)
		{
			var centerPiece = GameLogic.PiecesLogic.Pieces[centerTile.PieceId];
			var nextPiece = GameLogic.PiecesLogic.Pieces[nextTile.PieceId];
			
			if(!centerSlices.ContainsKey(color) || centerPiece.IsComplete || nextSlices.Count > 1)
			{
				transfer = default;

				return false;
			}
			
			var amount = GameLogic.PiecesLogic.TransferSlices(centerTile.PieceId, nextTile.PieceId, color);
			
			transfer = new PieceTransferData(centerTile.Id, nextTile.Id, centerTile.PieceId, nextPiece.Id, color, amount);
			
			if (!nextPiece.IsFull)
			{
				slicesCache.Add(color, nextPiece);
			}

			return true;
		}

		private bool TryTransferToCenterFromCache(ITileData centerTile, ITileData nextTile, 
			Dictionary<SliceColor, IPieceData> slicesCache, SliceColor color, out PieceTransferData transfer)
		{
			var centerPiece = GameLogic.PiecesLogic.Pieces[centerTile.PieceId];
			
			if(!slicesCache.TryGetValue(color, out var cachePiece) || cachePiece.Id == nextTile.PieceId || cachePiece.IsFull)
			{
				transfer = default;

				return false;
			}
			
			var centerColorAmount = centerPiece.GetSlicesCount(color);
			var maxSlices = cachePiece.SlicesFreeSpace - centerColorAmount;
			var amount = GameLogic.PiecesLogic.TransferSlices(nextTile.PieceId, centerTile.PieceId, color, maxSlices);
			
			transfer = new PieceTransferData(nextTile.Id, centerTile.Id, nextTile.PieceId, centerTile.PieceId, color, amount);

			if (centerPiece.IsFull || cachePiece.SlicesFreeSpace == centerColorAmount + amount)
			{
				slicesCache.Remove(color);
			}

			return true;
		}
	}
}
