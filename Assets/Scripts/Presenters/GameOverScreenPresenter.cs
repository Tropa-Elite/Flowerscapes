using Game.Messages;
using Game.Services;
using GameLovers.Services;
using GameLovers.UiService;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presenters
{
	public class GameOverScreenPresenter : UiPresenter
	{
		[SerializeField] private Button _restartButton;

		private IGameServices _services;

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();

			_restartButton.onClick.AddListener(Restart);
		}

		private void Restart()
		{
			_services.MessageBrokerService.Publish(new OnGameRestartClickedMessage());
		}
	}
}
