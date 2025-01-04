using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Freya;
using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using Game.ViewControllers;
using GameLovers.Services;
using Game.Commands;
using Game.Data;
using GameLovers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Controllers
{
	public interface IPiecesController
	{
		TileViewController OnPieceDrop(Vector2 screenPosition);
		void OnPieceDrag(Vector2 screenPosition);
		void DespawnPiece(PieceViewController piece);
	}
	
	public class PiecesController : IPiecesController
	{
		private readonly Dictionary<UniqueId, PieceViewController> _spawnedPieces = new(new UniqueIdKeyComparer());
		private readonly IGameServicesLocator _services;
		private readonly IGameDataProviderLocator _dataProvider;
		
		private PieceDeckViewController _deckViewController;
		private TileViewController _overingTile;

		public PiecesController(IGameServicesLocator services, IGameDataProviderLocator dataProvider)
		{
			_services = services;
			_dataProvider = dataProvider;
		}

		public async UniTask SetupAsync()
		{
			_deckViewController = Object.FindFirstObjectByType<PieceDeckViewController>();

			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDroppedMessage);
			
			await CreatePools();
		}

		public void Init()
		{
			CleanUpPieces();
			SpawnDeckPieces();
			SpawnBoardPieces();
		}

		public void CleanUp()
		{
			_services.MessageBrokerService.Unsubscribe<OnPieceDroppedMessage>(this);
			_services.PoolService.Dispose<SliceViewController>(true);
			_services.PoolService.Dispose<PieceViewController>(true);

			_deckViewController = null;
		}

		public TileViewController OnPieceDrop(Vector2 screenPosition)
		{
			var dataProvider = _dataProvider.GameplayBoardDataProvider;
			var tileOvering = GetTileFromPosition(screenPosition);
			
			_overingTile?.SetOveringState(false);

			// This means that it didn't drop over an empty tile or there is already a piece in it
			if (tileOvering == null || dataProvider.TryGetPieceFromTile(tileOvering.Row, tileOvering.Column, out _))
			{
				return null;
			}

			return tileOvering;
		}

		public void OnPieceDrag(Vector2 screenPosition)
		{
			var dataProvider = _dataProvider.GameplayBoardDataProvider;
			var tileOvering = GetTileFromPosition(screenPosition);
			
			// This means that there is already a piece where the player wants to drop it 
			if (tileOvering != null && dataProvider.TryGetPieceFromTile(tileOvering.Row, tileOvering.Column, out _))
			{
				tileOvering = null;
			}
			
			if (tileOvering != _overingTile)
			{
				_overingTile?.SetOveringState(false);
				tileOvering?.SetOveringState(true);
			}

			_overingTile = tileOvering;
		}

		public void DespawnPiece(PieceViewController piece)
		{
			_services.PoolService.Despawn(piece);
			_spawnedPieces.Remove(piece.Id);
		}

		private void OnPieceDroppedMessage(OnPieceDroppedMessage message)
		{
			if (_dataProvider.GameplayBoardDataProvider.PieceDeck.Count == Constants.Gameplay.Max_Deck_Pieces)
			{
				SpawnDeckPieces();
			}
			
			foreach (var transfer in message.TransferHistory)
			{
				var sourcePiece = _spawnedPieces[transfer.OriginPieceId];
				var targetPiece = _spawnedPieces[transfer.TargetPieceId];
				
				if(sourcePiece.GetSlicesCount(transfer.SliceColor) < transfer.SlicesAmount)
				{
					TransferSlicesDelay(sourcePiece, targetPiece, transfer.SliceColor, transfer.SlicesAmount).Forget();
					continue;
				}
				
				TransferSlices(sourcePiece, targetPiece, transfer.SliceColor, transfer.SlicesAmount);
			}
		}

		private async UniTaskVoid TransferSlicesDelay(PieceViewController sourcePiece, PieceViewController targetPiece,
			SliceColor color, int amount)
		{
			await UniTask.Delay((int) (Constants.Gameplay.Slice_Transfer_Tween_Time * 1000));

			while (sourcePiece.GetSlicesCount(color) < amount)
			{
				await UniTask.Delay((int) (Constants.Gameplay.Slice_Transfer_Delay_Time * 1000));
			}
			
			TransferSlices(sourcePiece, targetPiece, color, amount);
		}

		private void TransferSlices(PieceViewController sourcePiece, PieceViewController targetPiece, SliceColor color, int amount)
		{
			var parent = _services.PoolService.GetPool<SliceViewController>().SampleEntity.transform.parent;
			var targetStartIndex = targetPiece.GetNewSliceIndex(color, out var startRotation);
			var sourceStartIndex = sourcePiece.Slices.FindIndex(s => s.SliceColor == color);
			
			for (var i = 0; i < amount; i++)
			{
				var sourceIndex = sourceStartIndex + i;
				var targetIndex = targetStartIndex + i;
				var delay = i * Constants.Gameplay.Slice_Transfer_Delay_Time;
				var rotation = startRotation + Constants.Gameplay.Slice_Rotation * i;
				
				sourcePiece.Slices[sourceIndex].RectTransform.SetParent(parent);
				sourcePiece.Slices[sourceIndex].StartTransferAnimation(targetPiece, rotation, delay);
			}
			
			sourcePiece.Slices.RemoveRange(sourceStartIndex, amount);
			sourcePiece.AdjustRemainingSlicesAnimation(amount * Constants.Gameplay.Slice_Transfer_Delay_Time);
		}

		private async UniTask CreatePools()
		{
			var assetService = _services.AssetResolverService;
			var poolTransform = CreatePoolTransform();
			var pieceAddress = AddressableId.Addressables_Prefabs_Piece.GetConfig().Address;
			var sliceAddress = AddressableId.Addressables_Prefabs_Slice.GetConfig().Address;
			var initPosition = Vector3.right * 10000; // Move out of the screen
			var poolSize = Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns / 2;
			var piece = await assetService.InstantiateAsync(pieceAddress, initPosition, Quaternion.identity, poolTransform, DeactivateAsset);
			var slice = await assetService.InstantiateAsync(sliceAddress, initPosition, Quaternion.identity, poolTransform, DeactivateAsset);
			var poolSlices = new GameObjectPool<SliceViewController>(100, slice.GetComponent<SliceViewController>());
			var poolPieces = new GameObjectPool<PieceViewController>((uint)poolSize, piece.GetComponent<PieceViewController>(), PieceInstantiator);

			_services.PoolService.AddPool(poolPieces);
			_services.PoolService.AddPool(poolSlices);
		}

		private void DeactivateAsset(GameObject asset)
		{
			asset.SetActive(false);
		}

		private PieceViewController PieceInstantiator(PieceViewController piece)
		{
			var instance = GameObjectPool<PieceViewController>.Instantiator(piece);
			
			instance.Init(this);

			return instance;
		}

		private Transform CreatePoolTransform()
		{
			var poolTransform = new GameObject("PiecesController Pool").GetComponent<Transform>();
			var canvas = _deckViewController.GetComponentInParent<Canvas>();

			poolTransform.SetParent(canvas.transform);
			poolTransform.localPosition = Vector3.zero;
			poolTransform.localScale = Vector3.one;

			return poolTransform;
		}

		private void SpawnDeckPieces()
		{
			var distance = _deckViewController.RectTransform.rect.width / 4f;
			var xPos = -distance * 2;

			for (var i = 0; i < _dataProvider.GameplayBoardDataProvider.PieceDeck.Count; i++)
			{
				var pieceId = _dataProvider.GameplayBoardDataProvider.PieceDeck[i];
				
				xPos += distance;

				if (!pieceId.IsValid) continue;

				var piece = SpawnPiece(pieceId);

				piece.RectTransform.SetParent(_deckViewController.transform);
				piece.RectTransform.SetAsLastSibling();
				piece.AnimationSpawn(i * Constants.Gameplay.Piece_Spawn_Delay_Time);

				piece.RectTransform.anchoredPosition = new Vector3(xPos, 0, 0);
			}
		}

		private void SpawnBoardPieces() 
		{
			var tiles = Object.FindObjectsByType<TileViewController>(FindObjectsSortMode.None);
			var pieceCounter = 0;

			foreach (var tile in tiles)
			{
				if (_dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out var pieceData))
				{
					var piece = SpawnPiece(pieceData.Id);

					piece.MoveIntoTile(tile);
					piece.AnimationSpawn(pieceCounter * Constants.Gameplay.Piece_Spawn_Delay_Time);

					pieceCounter++;
				}
			}
		}

		private PieceViewController SpawnPiece(UniqueId pieceId)
		{
			var piece = _services.PoolService.Spawn<PieceViewController, UniqueId>(pieceId);
			
			_spawnedPieces.Add(pieceId, piece);
			
			return piece;
		}

		private void CleanUpPieces()
		{
			var pieces = _spawnedPieces.Keys.ToArray();
			
			foreach (var id in pieces)
			{
				DespawnPiece(_spawnedPieces[id]);
			}
		}

		private TileViewController GetTileFromPosition(Vector2 screenPosition)
		{
			if (!_deckViewController.CanvasRaycaster.RaycastPoint(screenPosition, out var hits))
			{
				return null;
			}

			var hit = hits.Find(x => x.gameObject.HasComponent<TileViewController>());

			// Is not allowed to put a piece on a tile with already a piece in it
			if (!hit.isValid)
			{
				return null;
			}
			
			var hitTile = hit.gameObject.GetComponent<TileViewController>();
			var limitDistance = hitTile.RectTransform.TransformVector(hitTile.RectTransform.rect.size).x * 3f / 5f;
			var hitTilePosition = (Vector2) hitTile.transform.position;

			// Avoid flickering the tile selection in the corners
			if (Mathfs.DistanceSquared(hitTilePosition, screenPosition) > limitDistance * limitDistance)
			{
				return null;
			}
			
			return hitTile;
		}
	}
}
