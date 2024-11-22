using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Logic.Shared;
using Game.Messages;
using Game.Utils;
using GameLovers.Services;
using System;
using System.Collections.Generic;

namespace Game.Commands
{
	/// <summary>
	/// This command is responsible to handle the logic when a piece is dropped in the board
	/// </summary>
	public struct PieceDropCommand : IGameCommand<IGameLogicLocator>
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
		public void Execute(IGameLogicLocator gameLogic, IMessageBrokerService messageBrokerService)
		{
			var boardLogic = gameLogic.GameplayBoardLogic;
			var nodeList = gameLogic.GameplayBoardLogic.GetPiecesNodeList(Row, Column);

			if (boardLogic.TryGetPieceFromTile(Row, Column, out _))
			{
				throw new LogicException($"There is already a piece on tile ({Row}, {Column})");
			}

			nodeList.Insert(0, new KeyValuePair<int, int>(Row, Column));
			boardLogic.SetPieceOnTile(PieceId, Row, Column);
			boardLogic.PieceDeck.Remove(PieceId);
			boardLogic.ActivateTile(Row, Column);

			if(boardLogic.PieceDeck.Count == 0)
			{
				boardLogic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			}

			// Update the piece changes
			for (var i = 0; i < nodeList.Count; i++)
			{
				if(gameLogic.GameplayBoardDataProvider.TryGetPieceFromTile(nodeList[i].Key, nodeList[i].Value, out var piece))
				{
					gameLogic.GameplayBoardLogic.Pieces.InvokeUpdate(piece.Id);
				}
			}

			messageBrokerService.Publish(new OnPieceDroppedMessage { PieceId = PieceId, Row = Row, Column = Column });

			if (boardLogic.IsGameOver())
			{
				messageBrokerService.Publish(new OnGameOverMessage());
			}
		}
	}
}
