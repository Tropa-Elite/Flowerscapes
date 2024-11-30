using Cysharp.Threading.Tasks;
using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using Game.ViewControllers;
using GameLovers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Controllers
{
	public class PiecesController
	{
		private readonly IGameServicesLocator _services;
		private readonly IGameDataProviderLocator _dataProvider;

		private GameObjectPool<PieceViewController> _pool;
		private PieceDeckViewController _deckViewController;

		public PiecesController(IGameServicesLocator services, IGameDataProviderLocator dataProvider)
		{
			_services = services;
			_dataProvider = dataProvider;
		}

		public async UniTask Setup()
		{
			var piece = await _services.AssetResolverService.InstantiateAsync(
				AddressableId.Addressables_Prefabs_Piece.GetConfig().Address,
				Vector3.right * 10000f, // Move out of the screen
				Quaternion.identity,
				CreatePoolTransform());

			_deckViewController = GameObject.FindFirstObjectByType<PieceDeckViewController>();

			CreatePiecesPool(piece.GetComponent<PieceViewController>());
			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDropped);
		}

		public void Init()
		{
			SpawnDeckPieces();
			InitializeBoardPieces();
		}

		public void CleanUp()
		{
			_pool.Dispose();
			GameObject.Destroy(_pool.SampleEntity.transform.parent.gameObject);
			_services.MessageBrokerService.Unsubscribe<OnPieceDroppedMessage>(this);

			_pool = null;
			_deckViewController = null;
		}

		private void OnPieceDropped(OnPieceDroppedMessage message)
		{
			// Check if the input board was just refilled
			if (_dataProvider.GameplayBoardDataProvider.PieceDeck.Count == Constants.Gameplay.MAX_DECK_PIECES)
			{
				SpawnDeckPieces();
			}
		}

		private void CreatePiecesPool(PieceViewController piece)
		{
			var poolSize = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS / 2;

			_pool = new GameObjectPool<PieceViewController>((uint)poolSize, piece);

			piece.gameObject.SetActive(false);
		}

		private Transform CreatePoolTransform()
		{
			var poolTransform = new GameObject("PiecePool").GetComponent<Transform>();

			poolTransform.SetParent(GameObject.FindFirstObjectByType<Canvas>().transform);
			poolTransform.localPosition = Vector3.zero;
			poolTransform.localScale = Vector3.one;

			return poolTransform;
		}

		private void SpawnDeckPieces()
		{
			var distance = _deckViewController.RectTransform.rect.width / 4f;
			var xPos = -distance * 2;

			foreach (var pieceId in _dataProvider.GameplayBoardDataProvider.PieceDeck)
			{
				xPos += distance;

				if (!pieceId.IsValid) continue;

				var piece = _pool.Spawn(pieceId);

				piece.RectTransform.SetParent(_deckViewController.transform);
				piece.RectTransform.SetAsLastSibling();

				piece.RectTransform.anchoredPosition = new Vector3(xPos, 0, 0);
			}
		}

		private void InitializeBoardPieces() 
		{
			var tiles = GameObject.FindObjectsByType<TileViewController>(FindObjectsSortMode.None);

			foreach (var tile in tiles)
			{
				if (_dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out var pieceData))
				{
					_pool.Spawn(pieceData.Id).MoveIntoTile(tile);
				}
			}
		}
	}
}
