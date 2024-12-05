using Game.Controllers;
using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Utils;
using Game.Views;
using GameLovers;
using GameLovers.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.ViewControllers
{
	public class PieceViewController : ViewControllerBase, IPointerUpHandler, IDragHandler,
		IPoolEntitySpawn<UniqueId>, IPoolEntityDespawn
	{
		[SerializeField] private DraggableViewController _draggableView;
		[SerializeField] private PieceSliceView[] _slices;
		[HideInInspector]
		[SerializeField] private GraphicRaycaster _canvasRaycaster;

		private IPiecesController _controller;
		private IGameDataProviderLocator _dataProvider;
		private UniqueId _uniqueId;

		public UniqueId Id => _uniqueId;
		public DraggableViewController DraggableView => _draggableView;

		protected override void OnEditorValidate()
		{
			_draggableView = _draggableView != null ? _draggableView : GetComponent<DraggableViewController>();
			_slices ??= GetComponentsInChildren<PieceSliceView>();
		}

		private void Awake()
		{
			var isMobile = Application.isMobilePlatform;
			
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_draggableView.DragSpeed = isMobile ? Constants.Gameplay.PIECE_MOBILE_SPEED : Constants.Gameplay.PIECE_DESKTOP_SPEED;
			_draggableView.Offset = isMobile ? Constants.Gameplay.PIECE_MOBILE_OFFSET : Constants.Gameplay.PIECE_DESKTOP_OFFSET;
			_canvasRaycaster = _canvasRaycaster != null ? _canvasRaycaster : GetComponentInParent<GraphicRaycaster>();
		}

		private void OnDestroy()
		{
			_dataProvider.PieceDataProvider.Pieces?.StopObserving(_uniqueId);
		}

		public void Init(IPiecesController controller)
		{
			_controller = controller;
		}

		/// <inheritdoc />
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_draggableView.enabled)
			{
				return;
			}

			if (!TryGetTileFromPosition(RectTransform.position, out var tile) ||
			    _dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out _))
			{
				_draggableView.ResetDraggable();
				_controller.OnPieceDrop(Id, null);
				return;
			}

			_draggableView.enabled = false;

			_draggableView.MoveIntoTransform(tile.transform);
			_controller.OnPieceDrop(Id, tile);
		}

		/// <inheritdoc />
		/// <remarks>
		/// This IDragHandler works because the same prefab contains <see cref="DraggableViewController"/> that
		/// implements the mandatory <see cref="IPointerDownHandler"/> interface
		/// </remarks>
		public void OnDrag(PointerEventData eventData)
		{
			if (!_draggableView.enabled)
			{
				return;
			}

			if (TryGetTileFromPosition(RectTransform.position, out var tile))
			{
				_controller.OnPieceDrag(tile);
			}
			else
			{
				_controller.OnPieceDrag(null);
			}
		}

		/// <inheritdoc />
		public void OnSpawn(UniqueId id)
		{
			_uniqueId = id;
			_draggableView.enabled = _dataProvider.GameplayBoardDataProvider.PieceDeck.Contains(id);

			_dataProvider.PieceDataProvider.Pieces.InvokeObserve(id, OnPieceUpdated);
		}

		/// <inheritdoc />
		public void OnDespawn()
		{
			_dataProvider.PieceDataProvider.Pieces.StopObserving(_uniqueId);

			_uniqueId = UniqueId.Invalid;
		}

		private void OnPieceUpdated(UniqueId id, IPieceData oldData, IPieceData newData, ObservableUpdateType updateType)
		{
			if (id != _uniqueId) return;

			switch (updateType)
			{
				case ObservableUpdateType.Updated:
					UpdateSlices();
					break;
				case ObservableUpdateType.Removed:
					_controller.Despawn(this);
					break;
			}
		}

		private void UpdateSlices()
		{
			var slices = _dataProvider.PieceDataProvider.Pieces[_uniqueId].Slices;

			for (var i = 0; i < Constants.Gameplay.MAX_PIECE_SLICES; i++)
			{
				if (i >= slices.Count)
				{
					_slices[i].gameObject.SetActive(false);
					continue;
				}

				_slices[i].SliceColor = slices[i];

				_slices[i].gameObject.SetActive(true);
			}
		}

		private bool TryGetTileFromPosition(Vector3 position, out TileViewController tile)
		{
			tile = null;
			
			if (!_canvasRaycaster.RaycastPoint(position, out var hits))
			{
				return false;
			}

			var hit = hits.Find(x => x.gameObject.HasComponent<TileViewController>());

			// Is not allowed to put a piece on a tile with already a piece in it
			if (!hit.isValid)
			{
				return false;
			}
			
			var hitTile = hit.gameObject.GetComponent<TileViewController>();
			var limitDistance = hitTile.RectTransform.TransformVector(hitTile.RectTransform.rect.size).x * 3f / 5f;

			// Avoid flickering the tile selection in the corners
			if (Freya.Mathfs.DistanceSquared(hitTile.RectTransform.position, position) > limitDistance * limitDistance)
			{
				return false;
			}

			tile = hitTile;

			return true;
		}
	}
}

