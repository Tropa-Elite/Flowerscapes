using Game.Logic;
using GameLovers.Services;

namespace Game.Commands
{
	/// <summary>
	/// This command is responsible to handle the logic when the game is restarted
	/// </summary>
	public struct RestartGameCommand : IGameCommand<IGameLogic>
	{
		/// <inheritdoc />
		public void Execute(IGameLogic gameLogic)
		{
			var logic = gameLogic.GameplayBoardLogic;

			logic.Pieces.Clear();
			logic.RefillBoard(gameLogic.EntityFactoryLogic.CreatePiece, gameLogic.RngLogic);
			logic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
		}
	}
}
