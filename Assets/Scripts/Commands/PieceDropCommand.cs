using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Logic.Shared;
using Game.Messages;
using Game.MonoComponent;
using Game.Utils;
using GameLovers;
using GameLovers.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Commands
{
	/// <summary>
	/// This command is responsible to handle the logic when a piece is dropped in the board
	/// </summary>
	public struct PieceDropCommand : IGameCommand<IGameLogic>
	{
		public UniqueId PieceId;
		public int Row;
		public int Column;

		public PieceDropCommand(UniqueId pieceId, int row, int column)
		{
			PieceId = pieceId;
			Row = row;
			Column = column;
		}

		/// <inheritdoc />
		public void Execute(IGameLogic gameLogic)
		{
			if (gameLogic.GameplayBoardDataProvider.TryGetPieceFromTile(Row, Column, out _))
			{
				throw new LogicException($"There is already a piece on tile ({Row}, {Column})");
			}

			gameLogic.GameplayBoardLogic.SetPieceOnTile(PieceId, Row, Column);
			gameLogic.GameplayBoardLogic.PieceDeck.Remove(PieceId);

			if(gameLogic.GameplayBoardLogic.PieceDeck.Count == 0)
			{
				gameLogic.GameplayBoardLogic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			}

			gameLogic.MessageBrokerService.Publish(new OnPieceDroppedMessage { PieceId = PieceId, Row = Row, Column = Column });
			/*
			// TODO: Check if there is any potential match first - Case B
			// TODO: Move this to GameplayBoardLogic

			var pieceData = gameLogic.GameplayBoardLogic.Pieces.GetOriginValue(PieceId);
			var pieceList = SorroundingPiecesList(gameLogic);
			var colorCount = ColorCount(pieceData);

			for (var i = 0; i < colorCount; i++)
			{
				var color = pieceData.Slices[i];
				var totalSlices = CollectSlicesFromTiles(pieceList, color);

				pieceList.Sort((x, y) => x.Piece.Slices.Count.CompareTo(y.Piece.Slices.Count));

				FillSlicesInTiles(pieceList, color, totalSlices);
			}

			CheckMatches(gameLogic, pieceList);*/
		}

		private void CheckMatches(IGameLogic gameLogic, List<TilePiece> list)
		{
			foreach (var tilePiece in list)
			{
				var piece = tilePiece.Piece;

				for (int i = 1; i < piece.Slices.Count; i++)
				{
					if (piece.Slices[i] != piece.Slices[0]) break;
					if (i < piece.Slices.Count - 1) continue;

					piece.Slices.Clear();
					gameLogic.GameplayBoardLogic.CleanUpTile(tilePiece.Row, tilePiece.Column);
				}
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

		private int CollectSlices(PieceData piece, SliceColor color)
		{
			var count = 1;

			for (int i = piece.Slices.Count - 1; i > -1; i--)
			{
				if (piece.Slices[i] == color)
				{
					count++;

					piece.Slices.RemoveAt(i);
				}
			}

			return count;
		}

		private void FillPiece(PieceData piece, SliceColor color, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				piece.Slices.Add(color);
			}
		}

		private int CollectSlicesFromTiles(List<TilePiece> list, SliceColor color)
		{
			var count = 0;

			foreach (var piece in list)
			{
				count += CollectSlices(piece.Piece, color);
			}

			return count;
		}

		private void FillSlicesInTiles(List<TilePiece> list, SliceColor color, int totalSlices)
		{
			foreach (var pieceTile in list)
			{
				var piece = pieceTile.Piece;
				var amount = Math.Clamp(totalSlices, piece.Slices.Count, Constants.Gameplay.MAX_PIECE_SLICES);

				totalSlices -= amount;

				FillPiece(piece, color, amount);
			}

			// TODO: Avoid this somehow
			if(totalSlices > 0)
			{
				throw new LogicException($"There are more slices than expected: {totalSlices}");
			}
		}

		private List<TilePiece> SorroundingPiecesList(IGameLogic gameLogic)
		{
			var list = new List<TilePiece>();

			if (gameLogic.GameplayBoardLogic.TryGetPieceDataFromTile(Row, Column, out var pieceCenter))
			{
				list.Add(new TilePiece { Row = Row, Column = Column, Piece = pieceCenter });
			}

			for (var i = -1; i < 2; i++)
			{
				if (i == 0) continue;
				if (gameLogic.GameplayBoardLogic.TryGetPieceDataFromTile(Row + i, Column, out var piece))
				{
					list.Add(new TilePiece { Row = Row, Column = Column, Piece = pieceCenter });
				}
			}

			for (var i = -1; i < 2; i++)
			{
				if(i == 0) continue;
				if (gameLogic.GameplayBoardLogic.TryGetPieceDataFromTile(Row, Column + 1, out var piece))
				{
					list.Add(new TilePiece { Row = Row, Column = Column, Piece = pieceCenter });
				}
			}

			return list;
		}

		private struct TilePiece
		{	
			public int Row;
			public int Column;
			public PieceData Piece;
		}
	}
}
