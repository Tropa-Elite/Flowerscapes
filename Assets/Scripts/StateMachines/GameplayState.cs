using System;
using System.Threading.Tasks;
using GameLovers.Services;
using GameLovers.StatechartMachine;
using Game.Ids;
using Game.Services;
using UnityEngine;
using Game.Messages;
using Game.MonoComponent;
using Game.Utils;
using Game.Logic;
using Game.Presenters;
using NUnit.Framework;
using System.Collections.Generic;
using Game.Commands;

namespace Game.StateMachines
{
	/// <summary>
	/// This object contains the behaviour logic for the Gameplay State in the <seealso cref="GameStateMachine"/>
	/// </summary>
	public class GameplayState
	{
		public static readonly IStatechartEvent GAME_OVER_EVENT = new StatechartEvent("Game Over Event");

		private static readonly IStatechartEvent RESTART_CLICKED_EVENT = new StatechartEvent("Restart Button Clicked Event");

		private readonly IGameUiService _uiService;
		private readonly IGameServices _services;
		private readonly IGameDataProvider _gameDataProvider;
		private readonly Action<IStatechartEvent> _statechartTrigger;
		
		public GameplayState(IInstaller installer, Action<IStatechartEvent> statechartTrigger)
		{
			_gameDataProvider = installer.Resolve<IGameDataProvider>();
			_services = installer.Resolve<IGameServices>();
			_uiService = installer.Resolve<IGameUiServiceInit>();
			_statechartTrigger = statechartTrigger;
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

			gameplay.Event(GAME_OVER_EVENT).Target(gameOver);

			gameOver.OnEnter(OpenGameOverUi);
			gameOver.Event(RESTART_CLICKED_EVENT).OnTransition(RestartGame).Target(gameStateCheck);
			gameOver.OnExit(CloseGameOverUi);

			final.OnEnter(UnsubscribeEvents);
		}

		private void SubscribeEvents()
		{
			_services.MessageBrokerService.Subscribe<OnGameOverMessage>(OnGameOverMessage);
			_services.MessageBrokerService.Subscribe<OnGameRestartClickedMessage>(OnGameRestartClickedMessage);
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void OnGameOverMessage(OnGameOverMessage message)
		{
			_statechartTrigger(GAME_OVER_EVENT);
		}

		private void OnGameRestartClickedMessage(OnGameRestartClickedMessage message)
		{
			_statechartTrigger(RESTART_CLICKED_EVENT);
		}

		private void GameInit()
		{
			_services.MessageBrokerService.Publish(new OnGameInitMessage());
		}

		private void RestartGame()
		{
			_services.CommandService.ExecuteCommand(new RestartGameCommand());
		}

		private bool IsGameOver()
		{
			return _gameDataProvider.GameplayBoardDataProvider.IsGameOver();
		}

		private void OpenGameOverUi()
		{
			_ = _uiService.OpenUiAsync<GameOverScreenPresenter>();
		}

		private void CloseGameOverUi()
		{
			_uiService.CloseUi<GameOverScreenPresenter>();
		}

		// TODO: Move this to a separate code file. Needs a pattern or structure for it
		private async Task LoadGameplayAssets()
		{
			var tasks = new List<Task> { _uiService.LoadGameUiSet(UiSetId.GameplayUi, 0.8f) };
			var poolTransform = new GameObject("PiecePool").GetComponent<Transform>();

			poolTransform.SetParent(GameObject.FindFirstObjectByType<Canvas>().transform);
			poolTransform.localPosition = Vector3.zero;
			poolTransform.localScale = Vector3.one;

			var piece = await _services.AssetResolverService.InstantiateAsync(
				Constants.Prefabs.PIECE, 
				Vector3.right * 10000f, // Move out of the screen
				Quaternion.identity,
				poolTransform);

			var piecePool = new GameObjectPool<PieceMonoComponent>(
				(uint) (Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS) / 2,
				piece.GetComponent<PieceMonoComponent>());

			piece.SetActive(false);
			_services.PoolService.AddPool(piecePool);

			await Task.WhenAll(tasks);

			GC.Collect();
			_ = Resources.UnloadUnusedAssets();
		}
	}
}