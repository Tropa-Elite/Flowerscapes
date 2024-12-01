using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Logic.Shared;
using Game.Messages;
using Game.Utils;
using GameLovers.Services;

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

			tileList.Insert(0, (_row, _column, gameLogic.PiecesLogic.Pieces[_pieceId]));
			boardLogic.SetPieceOnTile(_pieceId, _row, _column);
			boardLogic.PieceDeck.Remove(_pieceId);
			boardLogic.ActivateTile(_row, _column, gameLogic.PiecesLogic);

			if(boardLogic.PieceDeck.Count == 0)
			{
				boardLogic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			}

			// Update the piece changes
			for (var i = 0; i < tileList.Count; i++)
			{
				if(gameLogic.PiecesLogic.Pieces.ContainsKey(tileList[i].Item3.Id))
				{
					gameLogic.PiecesLogic.Pieces.InvokeUpdate(tileList[i].Item3.Id);
				}
			}

			messageBrokerService.Publish(new OnPieceDroppedMessage { PieceId = _pieceId, Row = _row, Column = _column });

			if (boardLogic.IsGameOver())
			{
				messageBrokerService.Publish(new OnGameOverMessage());
			}
		}
	}
}
