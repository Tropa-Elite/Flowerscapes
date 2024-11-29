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

		bool TryGetPieceFromTile(int row, int column, out IPieceData piece);

		bool IsGameOver();

		List<(int, int, IPieceData)> GetAdjacentTileList(int row, int column);
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableList<UniqueId> PieceDeck { get; }

		void SetPieceOnTile(UniqueId pieceId, int row, int column);

		void ActivateTile(int row, int column, IPiecesLogic pieceLogic);

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
		public bool TryGetPieceFromTile(int row, int column, out IPieceData piece)
		{
			piece = null;

			if (row < 0 || column < 0 || row >= Constants.Gameplay.BOARD_ROWS || column >= Constants.Gameplay.BOARD_COLUMNS)
			{
				return false;
			}

			var tile = Data.Board[row, column];

			if (tile == null || !GameDataProvider.PieceDataProvider.Pieces.TryGetValue(tile.Piece, out piece))
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
					if (Data.Board[i, j] == null || !Data.Board[i, j].Piece.IsValid)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc />
		public List<(int, int, IPieceData)> GetAdjacentTileList(int row, int column)
		{
			var list = new List<(int, int, IPieceData)>();

			for (var i = -1; i < 2; i++)
			{
				if (i != 0 && TryGetPieceFromTile(row + i, column, out var piece))
				{
					list.Add((row + i, column, piece));
				}
			}

			for (var i = -1; i < 2; i++)
			{
				if (i != 0 && TryGetPieceFromTile(row, column + i, out var piece))
				{
					list.Add((row, column + i, piece));
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
		public void ActivateTile(int row, int column, IPiecesLogic pieceLogic)
		{
			var piece = pieceLogic.Pieces[Data.Board[row, column].Piece];
			var adjacentTiles = GetAdjacentTileList(row, column);
			var pieceColors = piece.GetSlicesColors();
			var otherColorCache = new Dictionary<SliceColor, IPieceData>();
			var transferDone = false;
			var counter = 10;

			do
			{
				transferDone = false;

				foreach (var tile in adjacentTiles)
				{
					if (tile.Item3.IsFull || tile.Item3.IsEmpty) continue;

					transferDone = TryTransferSlices(piece, tile.Item3, pieceLogic, pieceColors, otherColorCache) || transferDone;
				}
			} 
			while (transferDone && counter-- > 0);

			adjacentTiles.Insert(0, (row, column, piece));

			foreach (var tile in adjacentTiles)
			{
				if (tile.Item3.Slices.Count == 0 || tile.Item3.Slices.Count == Constants.Gameplay.MAX_PIECE_SLICES)
				{
					CleanUpTile(tile.Item1, tile.Item2);
					pieceLogic.Pieces.Remove(tile.Item3.Id);
				}
			}
		}

		/// <inheritdoc />
		public void CleanUpTile(int row, int column)
		{
			if(Data.Board[row, column] != null)
			{
				Data.Board[row, column].Piece = UniqueId.Invalid;
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

		private bool TryTransferSlices(IPieceData piece, IPieceData pieceTile, IPiecesLogic pieceLogic,
			Dictionary<SliceColor, int> pieceColors, Dictionary<SliceColor, IPieceData> otherColorCache)
		{
			var tileColors = pieceTile.GetSlicesColors();

			foreach (var colorPair in tileColors)
			{
				var color = colorPair.Key;
				var canReceiveSlices = tileColors.Count > 1 && piece.SlicesFreeSpace > colorPair.Value;
				
				if (pieceColors.ContainsKey(color))
				{
					if (!piece.IsEmpty && !piece.IsFull && (pieceColors.Count == 1 || canReceiveSlices))
					{
						pieceColors[color] += pieceLogic.TransferSlices(pieceTile.Id, piece.Id, color);
						
						return true;
					}
					else if (tileColors.Count == 1)
					{
						pieceColors[color] -= pieceLogic.TransferSlices(piece.Id, pieceTile.Id, color);

						if (pieceColors[color] == 0)
						{
							pieceColors.Remove(color);
						}
						if (!pieceTile.IsFull)
						{
							otherColorCache.Add(color, pieceTile);
						}
						
						return true;
					}
				}
				else if (otherColorCache.TryGetValue(color, out var cachePiece) && 
					cachePiece.Id != pieceTile.Id && cachePiece.SlicesFreeSpace >0)
				{
					pieceColors[color] = pieceLogic.TransferSlices(pieceTile.Id, piece.Id, color, cachePiece.SlicesFreeSpace);

					otherColorCache.Remove(color);
					
					return true;
				}
			}

			return false;
		}
	}
}
