using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using Game.ViewControllers;
using GameLovers.Services;
using Game.Commands;
using Game.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Controllers
{
	public interface IPiecesController
	{
		void OnPieceDrop(UniqueId piece, TileViewController tile);
		void OnPieceDrag(TileViewController tileOvering);
	}
	
	public class PiecesController : IPiecesController
	{
		private readonly Dictionary<UniqueId, PieceViewController> _spawnedPieces = new(new UniqueIdKeyComparer());
		private readonly IGameServicesLocator _services;
		private readonly IGameDataProviderLocator _dataProvider;
		
		private GameObjectPool<PieceViewController> _poolPieces;
		private GameObjectPool<SliceViewController> _poolSlices;
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
			_poolPieces.Dispose();
			Object.Destroy(_poolPieces.SampleEntity.transform.parent.gameObject);
			_services.MessageBrokerService.Unsubscribe<OnPieceDroppedMessage>(this);

			_poolPieces = null;
			_deckViewController = null;
		}

		public void OnPieceDrop(UniqueId pieceId, TileViewController tile)
		{
			_overingTile?.SetOveringState(false);

			// This means that it dropped over a tile
			if (tile == null)
			{
				return;
			}
			
			_services.CommandService.ExecuteCommand(new PieceDropCommand(pieceId, tile.Row, tile.Column));
		}

		public void OnPieceDrag(TileViewController tileOvering)
		{
			var dataProvider = _dataProvider.GameplayBoardDataProvider;
			
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

		private void OnPieceDroppedMessage(OnPieceDroppedMessage message)
		{
			if (_dataProvider.GameplayBoardDataProvider.PieceDeck.Count == Constants.Gameplay.MAX_DECK_PIECES)
			{
				SpawnDeckPieces();
			}
			
			foreach (var transfer in message.TransferHistory)
			{
				var sourcePiece = _spawnedPieces[transfer.OriginPieceId];
				var targetPiece = _spawnedPieces[transfer.TargetPieceId];

				for(var i = 0; i < transfer.SlicesAmount; i++)
				{
					var slice = _poolSlices.Spawn(transfer.SliceColor);
					
					StartSliceTransferAnimation(sourcePiece, targetPiece, slice, i * 0.1f);
				}
			}
		}

		private void StartSliceTransferAnimation(PieceViewController fromPiece, PieceViewController toPiece, 
			SliceViewController slice, float delay)
		{
			var duration = Constants.Gameplay.SLICE_TRANSFER_TWEEN_TIME;
			var fromPieceClosure = fromPiece;
			var toPieceClosure = toPiece;
			var sliceClosure = slice;
			
			slice.RectTransform.position = fromPiece.RectTransform.position;
					
			slice.RectTransform.DOMove(toPieceClosure.RectTransform.position, duration)
				.SetDelay(delay)
				.OnStart(() => OnSliceTransferAnimationStarted(fromPieceClosure, sliceClosure))
				.OnComplete(() => OnSliceTransferAnimationCompleted(toPieceClosure, sliceClosure));
		}
		
		private void OnSliceTransferAnimationStarted(PieceViewController piece, SliceViewController slice)
		{
			piece.RemoveSlice(slice);
			
			if (!_dataProvider.PieceDataProvider.Pieces.ContainsKey(piece.Id) && piece.IsEmpty)
			{
				_poolPieces.Despawn(piece);
				_spawnedPieces.Remove(piece.Id);
			}
		}

		private void OnSliceTransferAnimationCompleted(PieceViewController piece, SliceViewController slice)
		{
			piece.AddSlice(slice);
			_poolSlices.Despawn(slice);
			
			// TODO: animate sort piece slices
			
			if (piece.IsComplete)
			{
				piece.AnimateComplete(_poolPieces);
				_spawnedPieces.Remove(piece.Id);
			}
		}

		private async UniTask CreatePools()
		{
			var assetService = _services.AssetResolverService;
			var poolTransform = CreatePoolTransform();
			var pieceAddress = AddressableId.Addressables_Prefabs_Piece.GetConfig().Address;
			var sliceAddress = AddressableId.Addressables_Prefabs_Slice.GetConfig().Address;
			var initPosition = Vector3.right * 10000; // Move out of the screen
			var poolSize = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS / 2;
			var piece = await assetService.InstantiateAsync(pieceAddress, initPosition, Quaternion.identity, poolTransform);
			var slice = await assetService.InstantiateAsync(sliceAddress, initPosition, Quaternion.identity, poolTransform);

			_poolPieces = new GameObjectPool<PieceViewController>((uint)poolSize, piece.GetComponent<PieceViewController>(), PieceInstantiator);
			_poolSlices = new GameObjectPool<SliceViewController>(100, slice.GetComponent<SliceViewController>());

			piece.gameObject.SetActive(false);
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

			poolTransform.SetParent(Object.FindFirstObjectByType<Canvas>().transform);
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

				var piece = SpawnPiece(pieceId);

				piece.RectTransform.SetParent(_deckViewController.transform);
				piece.RectTransform.SetAsLastSibling();

				piece.RectTransform.anchoredPosition = new Vector3(xPos, 0, 0);
			}
		}

		private void SpawnBoardPieces() 
		{
			var tiles = Object.FindObjectsByType<TileViewController>(FindObjectsSortMode.None);

			foreach (var tile in tiles)
			{
				if (_dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out var pieceData))
				{
					SpawnPiece(pieceData.Id).DraggableView.MoveIntoTransform(tile.transform);
				}
			}
		}

		private PieceViewController SpawnPiece(UniqueId pieceId)
		{
			var piece = _poolPieces.Spawn(pieceId);
			
			_spawnedPieces.Add(pieceId, piece);
			
			return piece;
		}

		private void CleanUpPieces()
		{
			foreach (var spawnedPiece in _spawnedPieces)
			{
				_poolPieces.Despawn(spawnedPiece.Value);
			}
			
			_spawnedPieces.Clear();
		}
	}
}
