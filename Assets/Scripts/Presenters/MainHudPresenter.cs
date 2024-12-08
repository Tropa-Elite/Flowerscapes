using GameLovers;
using GameLovers.Services;
using GameLovers.UiService;
using Game.Ids;
using Game.Logic;
using Game.Services;
using TMPro;
using UnityEngine;
using Game.Messages;
using UnityEngine.UI;

namespace Game.Presenters
{
	/// <summary>
	/// This Presenter handles the Main HUD UI by:
	/// - Showing the HUD visual status
	/// </summary>
	public class MainHudPresenter : UiPresenter
	{
		[SerializeField] private TextMeshProUGUI _softCurrencyText;
		[SerializeField] private TextMeshProUGUI _hardCurrencyText;
		[SerializeField] private Button _gameOverButton;

		private IGameDataProviderLocator _dataProvider;
		private IGameServicesLocator _services;

		private void Awake()
		{
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_services = MainInstaller.Resolve<IGameServicesLocator>();

			_gameOverButton.onClick.AddListener(GameOverClicked);
		}

		protected override void OnOpened()
		{
			_dataProvider.CurrencyDataProvider.Currencies.InvokeObserve(GameId.SoftCurrency, OnSoftCurrencyUpdated);
			_dataProvider.CurrencyDataProvider.Currencies.InvokeObserve(GameId.HardCurrency, OnHardCurrencyUpdated);
		}

		private void OnSoftCurrencyUpdated(GameId currency, int amountBefore, int amountAfter, ObservableUpdateType updateType)
		{
			_softCurrencyText.text = $"SC: {amountAfter.ToString()}";
		}

		private void OnHardCurrencyUpdated(GameId currency, int amountBefore, int amountAfter, ObservableUpdateType updateType)
		{
			_hardCurrencyText.text = $"HC: {amountAfter.ToString()}";
		}

		private void GameOverClicked()
		{
			_services.MessageBrokerService.Publish(new OnGameOverMessage());
		}
	}
}