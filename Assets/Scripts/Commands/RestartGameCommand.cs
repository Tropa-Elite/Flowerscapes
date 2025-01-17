using System;
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
		public Action<IGameLogicLocator> SetupTestData;
		
		/// <inheritdoc />
		public void Execute(IGameLogicLocator gameLogic, IMessageBrokerService messageBrokerService)
		{
			gameLogic.PiecesLogic.Pieces.Clear();
			gameLogic.TileBoardLogic.RefillBoard();
			gameLogic.DeckSpawnerLogic.RefillDeck();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			SetupTestData?.Invoke(gameLogic);
#endif
			
			messageBrokerService.Publish(new OnGameRestartMessage());
		}
	}
}
