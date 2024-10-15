using System;
using System.Threading.Tasks;
using GameLovers.Services;
using GameLovers.StatechartMachine;
using Game.Ids;
using Game.Services;
using UnityEngine;
using Game.Messages;
using Game.MonoComponent;

namespace Game.StateMachines
{
	/// <summary>
	/// This object contains the behaviour logic for the Gameplay State in the <seealso cref="GameStateMachine"/>
	/// </summary>
	public class GameplayState
	{
		private readonly IGameUiService _uiService;
		private readonly IGameServices _services;
		private readonly Action<IStatechartEvent> _statechartTrigger;
		
		public GameplayState(IGameServices services, IInstaller installer, Action<IStatechartEvent> statechartTrigger)
		{
			_services = services;
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
			var gameplay = stateFactory.State("Gameplay");
			
			initial.Transition().Target(gameplayLoading);
			initial.OnExit(SubscribeEvents);
			
			gameplayLoading.WaitingFor(LoadGameplayAssets).Target(gameplay);

			gameplay.OnEnter(OpenGameplayInit);

			final.OnEnter(UnsubscribeEvents);
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void SubscribeEvents()
		{
			// Add any events to subscribe
		}
		
		private async Task LoadGameplayAssets()
		{
			await _uiService.LoadGameUiSet(UiSetId.GameplayUi, 0.8f);

			var piece = await _services.AssetResolverService.InstantiateAsync(Constants.Prefabs.PIECE, Vector3.zero, Quaternion.identity, null);
			var piecePool = new GameObjectPool<PieceMonoComponent>((uint)Constants.NUMBER_OF_TILES, piece.GetComponent<PieceMonoComponent>());

			_services.PoolService.AddPool(piecePool);

			GC.Collect();
			_ = Resources.UnloadUnusedAssets();
		}

		private void OpenGameplayInit()
		{
			_uiService.OpenUiSet((int) UiSetId.GameplayUi, false);

			_services.MessageBrokerService.Publish(new OnGameInitMessage());
		}
	}
}