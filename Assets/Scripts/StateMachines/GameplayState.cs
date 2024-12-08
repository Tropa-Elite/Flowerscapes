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
		
		private static readonly IStatechartEvent PAUSE_CLICKED_EVENT = new StatechartEvent("Pause Clicked Event");
		private static readonly IStatechartEvent MENU_CLICKED_EVENT = new StatechartEvent("Menu Clicked Event");
		private static readonly IStatechartEvent CLOSE_CLICKED_EVENT = new StatechartEvent("Close Clicked Event");

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
			var pauseScreen = stateFactory.State("Pause Screen");

			initial.Transition().Target(gameplayLoading);
			initial.OnExit(SubscribeEvents);

			gameplayLoading.WaitingFor(LoadGameplayAssets).Target(gameStateCheck);

			gameStateCheck.OnEnter(GameInit);
			gameStateCheck.Transition().Condition(IsGameOver).Target(gameOver);
			gameStateCheck.Transition().Target(gameplay);

			gameplay.OnEnter(OpenGameplayUi);
			gameplay.Event(GAME_OVER_EVENT).Target(gameOver);
			gameplay.Event(PAUSE_CLICKED_EVENT).Target(pauseScreen);
			gameplay.OnExit(CloseGameplayUi);
			
			pauseScreen.OnEnter(OpenPauseScreenUi);
			pauseScreen.Event(GAME_OVER_EVENT).Target(gameOver);
			pauseScreen.Event(GAME_RESTART_EVENT).Target(gameStateCheck);
			pauseScreen.Event(CLOSE_CLICKED_EVENT).Target(gameplay);
			pauseScreen.Event(MENU_CLICKED_EVENT).Target(final);
			pauseScreen.OnExit(ClosePauseScreenUi);

			gameOver.OnEnter(OpenGameOverUi);
			gameOver.Event(GAME_RESTART_EVENT).Target(gameStateCheck);
			gameOver.OnExit(CloseGameOverUi);

			final.OnEnter(UnloadAssets);
			final.OnEnter(UnsubscribeEvents);
		}

		private void SubscribeEvents()
		{
			_services.MessageBrokerService.Subscribe<OnGameOverMessage>(OnGameOverMessage);
			_services.MessageBrokerService.Subscribe<OnGameRestartMessage>(OnGameRestartMessage);
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void OnGameOverMessage(OnGameOverMessage message)
		{
			_statechartTrigger(GAME_OVER_EVENT);
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

		private void OpenPauseScreenUi()
		{
			var data = new PauseScreenPresenter.PresenterData
			{
				OnReturnMenuClicked = () => _statechartTrigger(MENU_CLICKED_EVENT),
				OnCloseClicked = () => _statechartTrigger(CLOSE_CLICKED_EVENT)
			};
			
			_uiService.OpenUiAsync<PauseScreenPresenter, PauseScreenPresenter.PresenterData>(data).Forget();
		}

		private void ClosePauseScreenUi()
		{
			_uiService.CloseUi<PauseScreenPresenter>();
		}

		private void OpenGameplayUi()
		{
			var data = new MainHudPresenter.PresenterData
			{
				OnPauseClicked = () => _statechartTrigger(PAUSE_CLICKED_EVENT)
			};
			
			_uiService.OpenUiAsync<MainHudPresenter, MainHudPresenter.PresenterData>(data).Forget();
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
			await _piecesController.SetupAsync();
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