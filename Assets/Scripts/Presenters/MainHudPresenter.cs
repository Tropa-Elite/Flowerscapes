using GameLovers;
using GameLovers.Services;
using GameLovers.UiService;
using Game.Ids;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Presenters
{
	/// <summary>
	/// This Presenter handles the Main HUD UI by:
	/// - Showing the HUD visual status
	/// </summary>
	public class MainHudPresenter : UiPresenterData<MainHudPresenter.PresenterData>
	{
		public struct PresenterData
		{
			public UnityAction OnPauseClicked;
		}
		
		[SerializeField] private TextMeshProUGUI _softCurrencyText;
		[SerializeField] private TextMeshProUGUI _hardCurrencyText;
		[SerializeField] private Button _pauseButton;

		private IGameDataProviderLocator _dataProvider;

		private void Awake()
		{
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();

			_pauseButton.onClick.AddListener(() => Data.OnPauseClicked.Invoke());
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
	}
}