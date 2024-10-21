using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using GameLovers.Services;
using UnityEngine;

namespace Game.MonoComponent
{
	public class PieceSpawnerMonoComponent : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform;

		private IGameServices _services;
		private IGameDataProvider _dataProvider;

		private void OnValidate()
		{
			_rectTransform ??= GetComponent<RectTransform>();
		}

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();
			_dataProvider = MainInstaller.Resolve<IGameDataProvider>();

			_services.MessageBrokerService.Subscribe<OnGameInitMessage>(OnGameInit);
		}

		private void OnDestroy()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void OnGameInit(OnGameInitMessage message)
		{
			SpawnPieces();
		}

		private void SpawnPieces()
		{
			var distance = _rectTransform.rect.width / 4f;
			var xPos = -distance*2;

			foreach (var piece in _dataProvider.GameplayBoardDataProvider.InputPieces)
			{
				xPos += distance;

				if (!piece.IsValid) continue;

				var trans = _services.PoolService.Spawn<PieceMonoComponent, UniqueId>(piece).transform;

				trans.SetParent(transform);
				trans.SetAsLastSibling();

				trans.localPosition = new Vector3(xPos, 0, 0);
			}
		}
	}

}
