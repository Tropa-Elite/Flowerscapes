using System;
using GameLovers.Services;
using GameLovers.StatechartMachine;
using Game.Ids;
using Game.Services;
using UnityEngine;
using Game.Presenters;
using Game.Messages;
using Game.Commands;
using Game.Logic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Game.Utils;
using Game.Controllers;

namespace Game.StateMachines
{
	/// <summary>
	/// This object contains the behaviour logic for the Gameplay State in the <seealso cref="GameStateMachine"/>
	/// </summary>
	public class GameplayState
	{
		public static readonly IStatechartEvent GAME_OVER_EVENT = new StatechartEvent("Game Over Event");
		public static readonly IStatechartEvent GAME_RESTART_EVENT = new StatechartEvent("Game Restart Event");
		
		private static readonly IStatechartEvent MENU_CLICKED_EVENT = new StatechartEvent("Menu Clicked Event");

		private readonly IGameUiService _uiService;
		private readonly IGameServicesLocator _services;
		private readonly IGameDataProviderLocator _gameDataProvider;
		private readonly Action<IStatechartEvent> _statechartTrigger;
		private readonly PiecesController _piecesController;

		public GameplayState(IInstaller installer, Action<IStatechartEvent> statechartTrigger)
		{
			_gameDataProvider = installer.Resolve<IGameDataProviderLocator>();
			_services = installer.Resolve<IGameServicesLocator>();
			_uiService = installer.Resolve<IGameUiServiceInit>();
			_statechartTrigger = statechartTrigger;
			_piecesController = new PiecesController(_services, _gameDataProvider);
		}

		/// <summary>
		/// Setups the Adventure gameplay state
		/// </summary>
		public void Setup(IStateFactory stateFactory)
		{
			var initial = stateFactory.Initial("Initial");
			var final = stateFactory.Final("Final");
			var gameplayLoading = stateFactory.TaskWait("Gameplay Loading");
			var gameStateCheck = stateFactory.Choice("GameOver Check");
			var gameplay = stateFactory.State("Gameplay");
			var gameOver = stateFactory.State("GameOver");

			initial.Transition().Target(gameplayLoading);
			initial.OnExit(SubscribeEvents);

			gameplayLoading.WaitingFor(LoadGameplayAssets).Target(gameStateCheck);

			gameStateCheck.OnEnter(GameInit);
			gameStateCheck.Transition().Condition(IsGameOver).Target(gameOver);
			gameStateCheck.Transition().Target(gameplay);

			gameplay.OnEnter(OpenGameplayUi);
			gameplay.Event(GAME_OVER_EVENT).Target(gameOver);
			gameplay.OnExit(CloseGameplayUi);

			gameOver.OnEnter(OpenGameOverUi);
			gameOver.Event(GAME_RESTART_EVENT).Target(gameStateCheck);
			gameOver.Event(MENU_CLICKED_EVENT).Target(final);
			gameOver.OnExit(CloseGameOverUi);

			final.OnEnter(UnloadAssets);
			final.OnEnter(UnsubscribeEvents);
		}

		private void SubscribeEvents()
		{
			_services.MessageBrokerService.Subscribe<OnGameOverMessage>(OnGameOverMessage);
			_services.MessageBrokerService.Subscribe<OnGameRestartMessage>(OnGameRestartMessage);
			_services.MessageBrokerService.Subscribe<OnReturnMenuClickedMessage>(OnMenutClickedMessage);
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void OnGameOverMessage(OnGameOverMessage message)
		{
			_statechartTrigger(GAME_OVER_EVENT);
		}

		private void OnMenutClickedMessage(OnReturnMenuClickedMessage message)
		{
			_statechartTrigger(MENU_CLICKED_EVENT);
		}

		private void OnGameRestartMessage(OnGameRestartMessage message)
		{
			_statechartTrigger(GAME_RESTART_EVENT);
		}

		private void GameInit()
		{
			_piecesController.Init();
			_services.MessageBrokerService.Publish(new OnGameInitMessage());
		}

		private bool IsGameOver()
		{
			return _gameDataProvider.GameplayBoardDataProvider.IsGameOver();
		}

		private void OpenGameplayUi()
		{
			_uiService.OpenUiAsync<MainHudPresenter>().Forget();
		}

		private void CloseGameplayUi()
		{
			_uiService.CloseUi<MainHudPresenter>();
		}

		private void OpenGameOverUi()
		{
			_uiService.OpenUiAsync<GameOverScreenPresenter>().Forget();
		}

		private void CloseGameOverUi()
		{
			_uiService.CloseUi<GameOverScreenPresenter>();
		}

		private async UniTask LoadGameplayAssets()
		{
			await UniTask.WhenAll(
				_uiService.LoadGameUiSet(UiSetId.GameplayUi, 0.8f),
				_services.AssetResolverService.LoadSceneAsync(SceneId.Game, LoadSceneMode.Additive));
			await _piecesController.Setup();
		}

		private void UnloadAssets()
		{
			_piecesController.CleanUp();
			_uiService.UnloadGameUiSet(UiSetId.GameplayUi);
			_services.AssetResolverService.UnloadSceneAsync(SceneId.Game).Forget();
			Resources.UnloadUnusedAssets();
		}
	}
}