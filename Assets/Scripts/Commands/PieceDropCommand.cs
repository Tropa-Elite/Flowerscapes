using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Logic.Shared;
using Game.Messages;
using Game.Utils;
using GameLovers.Services;
using UnityEngine;

namespace Game.Commands
{
	/// <summary>
	/// This command is responsible to handle the logic when a piece is dropped in the board
	/// </summary>
	public readonly struct PieceDropCommand : IGameCommand<IGameLogicLocator>
	{
		private readonly UniqueId _pieceId;
		private readonly int _row;
		private readonly int _column;

		public PieceDropCommand(UniqueId pieceId, int row, int column)
		{
			_pieceId = pieceId;
			_row = row;
			_column = column;
		}

		/// <inheritdoc />
		public void Execute(IGameLogicLocator gameLogic, IMessageBrokerService messageBrokerService)
		{
			var boardLogic = gameLogic.GameplayBoardLogic;
			var tileList = gameLogic.GameplayBoardLogic.GetAdjacentTileList(_row, _column);

			if (boardLogic.TryGetPieceFromTile(_row, _column, out _))
			{
				throw new LogicException($"There is already a piece on tile ({_row}, {_column})");
			}

			boardLogic.PieceDeck.Remove(_pieceId);
			boardLogic.SetPieceOnTile(_pieceId, _row, _column);
			boardLogic.TryGetTileData(_row, _column, out var tile);
			tileList.Insert(0, tile);
			boardLogic.ActivateTile(_row, _column, gameLogic.PiecesLogic, out var transferHistory);

			if(boardLogic.PieceDeck.Count == 0)
			{
				boardLogic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			}

			// Update the piece changes
			for (var i = 0; i < tileList.Count; i++)
			{
				if(gameLogic.PiecesLogic.Pieces.ContainsKey(tileList[i].PieceId))
				{
					gameLogic.PiecesLogic.Pieces.InvokeUpdate(tileList[i].PieceId);
				}
			}

			messageBrokerService.Publish(new OnPieceDroppedMessage
			{
				PieceId = _pieceId, 
				Tile = tile, 
				TransferHistory = transferHistory
			});

			if (boardLogic.IsGameOver())
			{
				messageBrokerService.Publish(new OnGameOverMessage());
			}
		}
	}
}
