using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using GameLovers.Services;
using System;
using UnityEngine;

namespace Game.MonoComponent
{
	public class PieceDeckMonoComponent : MonoBehaviour
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
			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDropped);
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

			foreach (var pieceId in _dataProvider.GameplayBoardDataProvider.PieceDeck)
			{
				xPos += distance;

				if (!pieceId.IsValid) continue;

				var piece = _services.PoolService.Spawn<PieceMonoComponent, UniqueId>(pieceId);

				piece.RectTransform.SetParent(transform);
				piece.RectTransform.SetAsLastSibling();

				piece.RectTransform.anchoredPosition = new Vector3(xPos, 0, 0);
			}
		}

		private void OnPieceDropped(OnPieceDroppedMessage message)
		{
			// Check if the input board was just refilled
			if(_dataProvider.GameplayBoardDataProvider.PieceDeck.Count == Constants.Gameplay.MAX_DECK_PIECES)
			{
				SpawnPieces();
			}
		}
	}

}
