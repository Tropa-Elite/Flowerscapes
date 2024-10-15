using Game.Messages;
using Game.Services;
using GameLovers.Services; 
using UnityEngine;

namespace Game.MonoComponent
{
	public class PieceSpawnerMonoComponent : MonoBehaviour
	{
		private IGameServices _services;

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();

			_services.MessageBrokerService.Subscribe<OnGameInitMessage>(OnGameInit);
			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDropped);
		}

		private void OnDestroy()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void OnPieceDropped(OnPieceDroppedMessage message)
		{
			if (GetComponentsInChildren<PieceMonoComponent>().Length == 0)
			{
				SpawnPieces();
			}
		}

		private void OnGameInit(OnGameInitMessage message)
		{
			SpawnPieces();
		}

		private void SpawnPieces()
		{
			// TODO: Get the board transform size
			var size = 300;
			var startPos = -size / 2f;

			for (var i = 0; i < Constants.SPAWN_PIECES; i++)
			{
				var trans = _services.PoolService.Spawn<PieceMonoComponent>().transform;

				trans.localPosition = new Vector3(startPos + (size * i / 3f), 0, 0);
			}
		}
	}

}
