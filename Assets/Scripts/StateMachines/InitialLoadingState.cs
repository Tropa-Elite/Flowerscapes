using System.Threading.Tasks;
using Game.Data;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using GameLovers.StatechartMachine;
using GameLovers.UiService;
using Game.Ids;
using Game.Logic;
using Newtonsoft.Json;
using Game.Services;
using UnityEngine;
using System;

namespace Game.StateMachines
{
	/// <summary>
	/// This class represents the Loading state in the <seealso cref="GameStateMachine"/>
	/// </summary>
	internal class InitialLoadingState
	{
		private readonly IGameServices _services;
		private readonly IGameLogicInit _gameLogic;
		private readonly IGameUiServiceInit _uiService;
		private readonly IConfigsAdder _configsAdder;
		private readonly IDataService _dataService;

		public InitialLoadingState(IInstaller installer)
		{
			_gameLogic = installer.Resolve<IGameLogicInit>();
			_services = installer.Resolve<IGameServices>();
			_uiService = installer.Resolve<IGameUiServiceInit>();
			_configsAdder = installer.Resolve<IConfigsAdder>();
			_dataService = installer.Resolve<IDataService>();
		}

		/// <summary>
		/// Setups the Initial Loading state
		/// </summary>
		public void Setup(IStateFactory stateFactory)
		{
			var initial = stateFactory.Initial("Initial");
			var final = stateFactory.Final("Final");
			var dataLoading = stateFactory.TaskWait("Initial device data loading");
			var uiLoading = stateFactory.TaskWait("Initial Ui loading");
			
			initial.Transition().Target(dataLoading);
			initial.OnExit(SubscribeEvents);
			
			dataLoading.OnEnter(InitPlugins);
			dataLoading.WaitingFor(LoadConfigs).Target(uiLoading);
			dataLoading.OnExit(InitGameLogic);
			
			uiLoading.WaitingFor(LoadInitialUi).Target(final);
			
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

		private void InitPlugins()
		{
			if (Debug.isDebugBuild)
			{
				//SRDebug.Init();
			}
		}

		private async Task LoadInitialUi()
		{
			//await Task.WhenAll(_uiService.LoadUiSetAsync((int) UiSetId.InitialLoadUi));
			await Task.Yield();
		}

		private async Task LoadConfigs()
		{
			/*var uiConfigs = await _services.AssetResolverService.LoadAssetAsync<UiConfigs>(AddressableId.Addressables_Configs_UiConfigs.GetConfig().Address);
			var gameConfigs = await _services.AssetResolverService.LoadAssetAsync<GameConfigs>(AddressableId.Addressables_Configs_GameConfigs.GetConfig().Address);
			var dataConfigs = await _services.AssetResolverService.LoadAssetAsync<DataConfigs>(AddressableId.Addressables_Configs_DataConfigs.GetConfig().Address);
			
			_uiService.Init(uiConfigs);
			_configsAdder.AddSingletonConfig(gameConfigs.Config);
			_configsAdder.AddConfigs(data => (int) data.Id, dataConfigs.Configs);
			
			_services.AssetResolverService.UnloadAsset(uiConfigs);
			_services.AssetResolverService.UnloadAsset(gameConfigs);
			_services.AssetResolverService.UnloadAsset(dataConfigs);*/
			await Task.Yield();
		}

		private void InitGameLogic()
		{
			_gameLogic.Init(_dataService, _services);
		}
	}
}