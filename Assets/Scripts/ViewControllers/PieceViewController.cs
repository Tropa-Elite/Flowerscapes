using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Commands;
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
			_draggableView.PivotOffset = isMobile ? Constants.Gameplay.Draggable_Mobile_Pivot_Offset : Constants.Gameplay.Draggable_Desktop_Pivot_Offset;
		}

		private void OnDestroy()
		{
			_dataProvider.PieceDataProvider.Pieces?.StopObserving(_uniqueId);
		}

		public void Init(IPiecesController controller)
		{
			_controller = controller;
		}

		public int GetNewSliceIndex(SliceColor color, out Vector3 rotation)
		{
			var index = Slices.FindLastIndex(s => s.SliceColor == color);

			index = index < 0 ? Slices.Count : index + 1;
			rotation = Slices[index - 1].RectTransform.rotation.eulerAngles + Constants.Gameplay.Slice_Rotation;
			
			return index;
		}

		public int GetSlicesCount(SliceColor color)
		{
			var count = 0;
			
			foreach (var slice in Slices)
			{
				count += slice.SliceColor == color ? 1 : 0;
			}

			return count;
		}

		/// <inheritdoc />
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_draggableView.enabled)
			{
				return;
			}

			var tile = _controller.OnPieceDrop(RectTransform.TransformPoint(RectTransform.rect.center));
			
			if (tile.IsValid())
			{
				_draggableView.enabled = false;
				
				MoveIntoTile(tile);
				_services.CommandService.ExecuteCommand(new PieceDropCommand(Id, tile.Row, tile.Column));
			}
			else
			{
				_draggableView.ResetPosition();
			}
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
			
			_controller.OnPieceDrag(RectTransform.TransformPoint(RectTransform.rect.center));
		}

		/// <inheritdoc />
		public void OnSpawn(UniqueId id)
		{
			var slices = _dataProvider.PieceDataProvider.Pieces[id].Slices;

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
				
				slice.RectTransform.localScale = Vector3.one;
			}
			
			_uniqueId = id;
			_draggableView.enabled = false;
			RectTransform.localScale = Vector3.zero;
		}

		/// <inheritdoc />
		public void OnDespawn()
		{
			_uniqueId = UniqueId.Invalid;
			
			foreach (var slice in Slices)
			{
				_services.PoolService.Despawn(slice);
			}
			
			RectTransform.DOKill();
			Slices.Clear();
		}

		public void MoveIntoTile(TileViewController tile)
		{
			RectTransform.SetParent(tile.transform);
			RectTransform.SetAsLastSibling();
			DraggableView.ResetPivot();

			RectTransform.anchoredPosition = Vector3.zero;
		}
		
		public void AddSlice(SliceViewController slice)
		{
			var index = GetNewSliceIndex(slice.SliceColor, out var rotation);
			var tweener = slice.StartRotateAnimation(rotation);
			
			slice.RectTransform.SetParent(transform);
			slice.RectTransform.SetLocalPositionAndRotation(Vector3.zero, slice.RectTransform.localRotation);
			Slices.Insert(index, slice);
			
			if (index < Constants.Gameplay.Max_Piece_Slices - 1 || !IsComplete) return;

			if (tweener.IsValid())
			{
				tweener.OnComplete(AnimationComplete);
			}
			else
			{
				AnimationComplete();
			}
		}

		public void AdjustRemainingSlicesAnimation(float delay)
		{
			if (!_dataProvider.PieceDataProvider.Pieces.ContainsKey(_uniqueId) && IsEmpty)
			{
				_services.CoroutineService.StartDelayCall(_controller.DespawnPiece, this, delay);
				return;
			}
			
			for (var i = 1; i < Slices.Count; i++)
			{
				var newRotation = Slices[i - 1].RectTransform.rotation.eulerAngles + Constants.Gameplay.Slice_Rotation;
				
				Slices[i].StartRotateAnimation(newRotation);
			}
		}

		private void AnimationComplete()
		{
			for (var i = 0; i < Slices.Count; i++)
			{
				Slices[i].RectTransform.DOKill();
			}
			
			RectTransform.DOScale(Vector3.zero, Constants.Gameplay.Piece_Complete_Tween_Time)
				.SetEase(Ease.InBack)
				.OnComplete(() => _controller.DespawnPiece(this));
		}

		public void AnimationSpawn(float delay)
		{
			RectTransform.DOScale(Vector3.one, Constants.Gameplay.Piece_Spawn_Tween_Time)
				.SetDelay(delay)
				.SetEase(Ease.OutBack)
				.OnComplete(() =>
				{
					_draggableView.enabled = _dataProvider.DeckSpawnerDataProvider.Deck.Contains(_uniqueId);
				});
		}
	}
}

