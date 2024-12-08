using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Controllers;
using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Utils;
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
		[SerializeField] private List<SliceViewController> _slices;
		[HideInInspector]
		[SerializeField] private GraphicRaycaster _canvasRaycaster;

		private IPiecesController _controller;
		private IGameDataProviderLocator _dataProvider;
		private UniqueId _uniqueId;

		public UniqueId Id => _uniqueId;
		public DraggableViewController DraggableView => _draggableView;
		public bool IsFull => _slices.TrueForAll(slice => slice.isActiveAndEnabled);
		public bool IsEmpty => _slices.TrueForAll(slice => !slice.isActiveAndEnabled);
		public bool IsComplete => IsFull && _slices.Select(s => s.SliceColor).Distinct().Count() == 1;

		protected override void OnEditorValidate()
		{
			_draggableView = _draggableView != null ? _draggableView : GetComponent<DraggableViewController>();
			_slices = _slices.Count == 0 ? new List<SliceViewController>(GetComponentsInChildren<SliceViewController>()) : _slices;
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

			UpdateSlices();
		}

		/// <inheritdoc />
		public void OnDespawn()
		{
			_uniqueId = UniqueId.Invalid;
		}

		public void AddSlice(SliceViewController newSlice)
		{
			var index = _slices.FindLastIndex(slice => slice.isActiveAndEnabled && slice.SliceColor == newSlice.SliceColor);
			var lastColor = newSlice.SliceColor;

			for (var i = index + 1; i < Constants.Gameplay.MAX_PIECE_SLICES; i++)
			{
				var color = _slices[i].SliceColor;
				
				_slices[i].SliceColor = lastColor;
				lastColor = color;
				
				if (!_slices[i].isActiveAndEnabled)
				{
					_slices[i].gameObject.SetActive(true);
					break;
				}
			}
		}

		public void RemoveSlice(SliceViewController oldSlice)
		{
			var shifted = false;

			for (var i = 0; i < _slices.Count - 1; i++)
			{
				if (_slices[i].SliceColor == oldSlice.SliceColor && _slices[i + 1].SliceColor != oldSlice.SliceColor)
				{
					shifted = true;
				}

				if (shifted)
				{
					_slices[i].OnSpawn(_slices[i + 1].SliceColor);
				}

				if (_slices[i + 1].IsDisabled)
				{
					_slices[i].Disable();
					break;
				}
			}
		}

		private void UpdateSlices()
		{
			var slices = _dataProvider.PieceDataProvider.Pieces[_uniqueId].Slices;

			for (var i = 0; i < Constants.Gameplay.MAX_PIECE_SLICES; i++)
			{
				if (i >= slices.Count)
				{
					_slices[i].Disable();
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

		public void AnimateComplete(IObjectPool<PieceViewController> pool)
		{
			var duration = Constants.Gameplay.PIECE_COMPLETE_TWEEN_TIME;
			var delay = Constants.Gameplay.PIECE_DELAY_TWEEN_TIME;
			var poolClosure = pool;
			
			RectTransform.DOPunchScale(Vector3.one * 1.5f, duration).SetDelay(delay).OnComplete(() => poolClosure.Despawn(this));
		}
	}
}

