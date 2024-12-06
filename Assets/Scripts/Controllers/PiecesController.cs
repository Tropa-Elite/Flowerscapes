using Cysharp.Threading.Tasks;
using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using Game.ViewControllers;
using GameLovers.Services;
using Game.Commands;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Controllers
{
	public interface IPiecesController
	{
		void Despawn(PieceViewController piece);
		void OnPieceDrop(UniqueId piece, TileViewController tile);
		void OnPieceDrag(TileViewController tileOvering);
	}
	
	public class PiecesController : IPiecesController
	{
		private readonly IGameServicesLocator _services;
		private readonly IGameDataProviderLocator _dataProvider;

		private GameObjectPool<PieceViewController> _pool;
		private PieceDeckViewController _deckViewController;
		private TileViewController _overingTile;

		public PiecesController(IGameServicesLocator services, IGameDataProviderLocator dataProvider)
		{
			_services = services;
			_dataProvider = dataProvider;
		}

		public async UniTask SetupAsync()
		{
			var piece = await _services.AssetResolverService.InstantiateAsync(
				AddressableId.Addressables_Prefabs_Piece.GetConfig().Address,
				Vector3.right * 10000f, // Move out of the screen
				Quaternion.identity,
				CreatePoolTransform());

			_deckViewController = Object.FindFirstObjectByType<PieceDeckViewController>();

			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDroppedMessage);
			CreatePiecesPool(piece.GetComponent<PieceViewController>());
		}

		public void Init()
		{
			SpawnDeckPieces();
			InitializeBoardPieces();
		}

		public void CleanUp()
		{
			_pool.Dispose();
			Object.Destroy(_pool.SampleEntity.transform.parent.gameObject);
			_services.MessageBrokerService.Unsubscribe<OnPieceDroppedMessage>(this);

			_pool = null;
			_deckViewController = null;
		}

		public void Despawn(PieceViewController piece)
		{
			_pool.Despawn(piece);
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
		}

		private void CreatePiecesPool(PieceViewController piece)
		{
			var poolSize = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS / 2;

			_pool = new GameObjectPool<PieceViewController>((uint)poolSize, piece, PieceInstantiator);

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
			var poolTransform = new GameObject("PiecePool").GetComponent<Transform>();

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

				var piece = _pool.Spawn(pieceId);

				piece.RectTransform.SetParent(_deckViewController.transform);
				piece.RectTransform.SetAsLastSibling();

				piece.RectTransform.anchoredPosition = new Vector3(xPos, 0, 0);
			}
		}

		private void InitializeBoardPieces() 
		{
			var tiles = Object.FindObjectsByType<TileViewController>(FindObjectsSortMode.None);

			foreach (var tile in tiles)
			{
				if (_dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out var pieceData))
				{
					_pool.Spawn(pieceData.Id).DraggableView.MoveIntoTransform(tile.transform);
				}
			}
		}
	}
}
