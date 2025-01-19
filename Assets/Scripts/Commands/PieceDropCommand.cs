using System.Collections.Generic;
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
			var boardLogic = gameLogic.TileBoardLogic;

			if (boardLogic.TryGetPieceFromTile(_row, _column, out _))
			{
				throw new LogicException($"There is already a piece on tile ({_row}, {_column})");
			}

			gameLogic.DeckSpawnerLogic.Deck.Remove(_pieceId);
			boardLogic.SetPieceOnTile(_pieceId, _row, _column);
			boardLogic.ActivateTile(_row, _column, out var tiles, out var transfers);
			ProcessCompleted(gameLogic, tiles);

			if(gameLogic.DeckSpawnerLogic.Deck.Count == 0)
			{
				gameLogic.DeckSpawnerLogic.RefillDeck();
			}

			messageBrokerService.Publish(new OnPieceDroppedMessage
			{
				PieceId = _pieceId, 
				TileId = TileData.ToTileId(_row, _column), 
				TransferHistory = transfers
			});

			if(gameLogic.GameLevelDataProvider.IsLevelCompleted())
			{
				messageBrokerService.Publish(new OnGameCompleteMessage());
			}
			else if (gameLogic.GameLevelDataProvider.IsGameOver())
			{
				messageBrokerService.Publish(new OnGameOverMessage());
			}
		}

		private void ProcessCompleted(IGameLogicLocator gameLogic, List<ITileData> tiles)
		{
			foreach (var nextTile in tiles)
			{
				var piece = gameLogic.PiecesLogic.Pieces[nextTile.PieceId];
				
				if (piece.Slices.Count > 0 && piece.Slices.Count < Constants.Gameplay.Max_Piece_Slices)
				{
					continue;
				}
				
				gameLogic.PiecesLogic.Pieces.Remove(nextTile.PieceId);
				gameLogic.TileBoardLogic.CleanUpTile(nextTile.Row, nextTile.Column);

				if (piece.Slices.Count == Constants.Gameplay.Max_Piece_Slices)
				{
					// TODO: Add the experience based on the level of the flower completed
					gameLogic.GameLevelLogic.AddLevelXp(Constants.Gameplay.Level_Piece_Xp);
				}
			}
		}
	}
}
