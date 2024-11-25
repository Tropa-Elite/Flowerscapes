using Game.Data;
using Game.Logic;
using Game.Messages;
using GameLovers.Services;
using System.Collections.Generic;

namespace Game.Commands
{
	/// <summary>
	/// This command is responsible to handle the logic when the game is restarted
	/// </summary>
	public struct RestartGameCommand : IGameCommand<IGameLogicLocator>
	{
		/// <inheritdoc />
		public void Execute(IGameLogicLocator gameLogic, IMessageBrokerService messageBrokerService)
		{
			var logic = gameLogic.GameplayBoardLogic;

			gameLogic.PiecesLogic.Pieces.Clear();
			gameLogic.GameplayBoardLogic.RefillBoard(gameLogic.EntityFactoryLogic.CreatePiece, gameLogic.RngLogic);
			gameLogic.GameplayBoardLogic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			SetupTestData(gameLogic);
			messageBrokerService.Publish(new OnGameRestartMessage());
		}

		private void SetupTestData(IGameLogicLocator gameLogic)
		{
			gameLogic.PiecesLogic.Pieces.GetOriginValue(gameLogic.GameplayBoardLogic.PieceDeck[0]).Slices = new List<SliceColor> 
			{ 
				SliceColor.Green, SliceColor.Green, SliceColor.White, SliceColor.White
			};

			if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 0, out var piece1))
			{
				gameLogic.PiecesLogic.Pieces.Remove(piece1.Id);
				gameLogic.GameplayBoardLogic.CleanUpTile(0, 0);
			}
			if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 1, out var piece2))
			{
				gameLogic.PiecesLogic.Pieces.Remove(piece2.Id);
				gameLogic.GameplayBoardLogic.CleanUpTile(0, 1);
			}
			if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 2, out var piece3))
			{
				gameLogic.PiecesLogic.Pieces.Remove(piece3.Id);
				gameLogic.GameplayBoardLogic.CleanUpTile(0, 2);
			}
			var piece = gameLogic.EntityFactoryLogic.CreatePiece();
			gameLogic.GameplayBoardLogic.SetPieceOnTile(piece.Id, 0, 0);
			piece.Slices = new List<SliceColor> { SliceColor.Red, SliceColor.Red, SliceColor.White, SliceColor.White };
			piece = gameLogic.EntityFactoryLogic.CreatePiece();
			gameLogic.GameplayBoardLogic.SetPieceOnTile(piece.Id, 0, 2);
			piece.Slices = new List<SliceColor> { SliceColor.Red, SliceColor.Red, SliceColor.White, SliceColor.White };
		}
	}
}
