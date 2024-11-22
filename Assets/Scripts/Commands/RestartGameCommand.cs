using Game.Logic;
using Game.Messages;
using GameLovers.Services;

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

			logic.Pieces.Clear();
			logic.RefillBoard(gameLogic.EntityFactoryLogic.CreatePiece, gameLogic.RngLogic);
			logic.RefillPieceDeck(gameLogic.EntityFactoryLogic.CreatePiece);
			messageBrokerService.Publish(new OnGameRestartMessage());
		}
	}
}
