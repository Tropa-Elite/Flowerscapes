using GameLovers;
using GameLovers.Services;
using GameLovers.UiService;
using Game.Ids;
using Game.Logic;
using Game.Utils;
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
	public class MainHudPresenter : UiPresenter<MainHudPresenter.PresenterData>
	{
		public struct PresenterData
		{
			public UnityAction OnPauseClicked;
		}
		
		[SerializeField] private TextMeshProUGUI _currencyText;
		[SerializeField] private TextMeshProUGUI _levelText;
		[SerializeField] private TextMeshProUGUI _progressText;
		[SerializeField] private Slider _progressSlider;
		[SerializeField] private Button _pauseButton;
		[SerializeField] private GameObject _rewardIcon;
		[SerializeField] private GameObject _completedIcon;

		private IGameDataProviderLocator _dataProvider;

		private void Awake()
		{
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_progressSlider.maxValue = Constants.Gameplay.Level_Max_Xp;

			_dataProvider.GameLevelDataProvider.LevelXp.InvokeObserve(OnLevelXpUpdated);
			_pauseButton.onClick.AddListener(() => Data.OnPauseClicked.Invoke());
			_completedIcon.SetActive(_dataProvider.GameLevelDataProvider.IsLevelCompleted());
			_rewardIcon.SetActive(!_completedIcon.activeSelf);
		}

		protected override void OnOpened()
		{
			_dataProvider.CurrencyDataProvider.Currencies.InvokeObserve(GameId.SoftCurrency, OnCurrencyUpdated);
		}

		private void OnCurrencyUpdated(GameId currency, int amountBefore, int amountAfter, ObservableUpdateType updateType)
		{
			_currencyText.text = $"SC: {amountAfter.ToString()}";
		}

		private void OnLevelXpUpdated(int oldValue, int newValue)
		{
			_progressText.text = $"{newValue}/{Constants.Gameplay.Level_Max_Xp}";
			_progressSlider.value = newValue;
		}
	}
}