using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Controllers;
using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Services;
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
		[HideInInspector]
		[SerializeField] private GraphicRaycaster _canvasRaycaster;

		private IGameServicesLocator _services;
		private IGameDataProviderLocator _dataProvider;
		private IPiecesController _controller;
		private UniqueId _uniqueId;

		public UniqueId Id => _uniqueId;
		public DraggableViewController DraggableView => _draggableView;
		public List<SliceViewController> Slices { get; } = new ();
		public bool IsFull => Slices.Count == Constants.Gameplay.Max_Piece_Slices;
		public bool IsEmpty => Slices.Count == 0;
		public bool IsComplete => IsFull && Slices.Select(s => s.SliceColor).Distinct().Count() == 1;

		protected override void OnEditorValidate()
		{
			_draggableView = _draggableView != null ? _draggableView : GetComponent<DraggableViewController>();
		}

		private void Awake()
		{
			var isMobile = Application.isMobilePlatform;
			
			_services = MainInstaller.Resolve<IGameServicesLocator>();
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_draggableView.DragSpeed = isMobile ? Constants.Gameplay.Piece_Mobile_Speed : Constants.Gameplay.Piece_Desktop_Speed;
			_draggableView.Offset = isMobile ? Constants.Gameplay.Piece_Mobile_Offset : Constants.Gameplay.Piece_Desktop_Offset;
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
			var slices = _dataProvider.PieceDataProvider.Pieces[id].Slices;
			
			_uniqueId = id;
			_draggableView.enabled = _dataProvider.GameplayBoardDataProvider.PieceDeck.Contains(id);

			for (var i = 0; i < Constants.Gameplay.Max_Piece_Slices; i++)
			{
				if (i >= slices.Count)
				{
					break;
				}

				var slice = _services.PoolService.Spawn<SliceViewController, SliceColor>(slices[i]);
				var rotation = Quaternion.Euler(Constants.Gameplay.Slice_Rotation * i);

				slice.RectTransform.SetParent(transform);
				slice.RectTransform.SetLocalPositionAndRotation(Vector3.zero, rotation);
				Slices.Add(slice);
			}
		}

		/// <inheritdoc />
		public void OnDespawn()
		{
			_uniqueId = UniqueId.Invalid;
			
			foreach (var slice in Slices)
			{
				_services.PoolService.Despawn(slice);
			}
			
			Slices.Clear();
		}

		public int GetNextSliceIndex(SliceColor color)
		{
			var index = Slices.FindLastIndex(s => s.SliceColor == color);

			return index < 0 ? Slices.Count : index + 1;
		}
		
		public void AddSlice(int index, SliceViewController slice)
		{
			slice.RectTransform.SetParent(transform);
			Slices.Insert(index, slice);
			AdjustPieceAnimation(0);
		}

		public void AdjustPieceAnimation(float delay)
		{
			if (!_dataProvider.PieceDataProvider.Pieces.ContainsKey(_uniqueId) && IsEmpty)
			{
				_services.CoroutineService.StartDelayCall(_controller.DespawnPiece, this, delay);
				return;
			}
			
			if (!_dataProvider.PieceDataProvider.Pieces.ContainsKey(_uniqueId) && IsComplete)
			{
				AnimateComplete();
				return;
			}
			
			for (var i = 0; i < Slices.Count; i++)
			{
				Slices[i].StartRotateAnimation(i);
			}
		}

		private void AnimateComplete()
		{
			for (var i = 0; i < Slices.Count; i++)
			{
				Slices[i].RectTransform.DOKill();
			}
			
			RectTransform.DOPunchScale(Vector3.one * 1.5f, Constants.Gameplay.Piece_Complete_Tween_Time)
				.SetDelay(Constants.Gameplay.Piece_Complete_Delay_Time)
				.OnComplete(() => _controller.DespawnPiece(this));
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

