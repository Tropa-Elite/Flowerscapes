using Game.Commands;
using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Services;
using Game.Utils;
using Game.Views;
using GameLovers;
using GameLovers.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.ViewControllers
{
	public class PieceViewController : 
		MonoBehaviour, IPointerUpHandler,
		IPoolEntityObject<PieceViewController>, IPoolEntitySpawn<UniqueId>, IPoolEntityDespawn
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private DraggableView _draggable;
		[SerializeField] private PieceSliceView[] _slices;
		[HideInInspector]
		[SerializeField] private GraphicRaycaster _canvasRaycaster;

		private IObjectPool<PieceViewController> _pool;
		private IGameServicesLocator _services;
		private IGameDataProviderLocator _dataProvider;
		private UniqueId _uniqueId;

		public RectTransform RectTransform => _rectTransform;
		public UniqueId Id => _uniqueId;

		void OnValidate()
		{
			_rectTransform = _rectTransform != null ? _rectTransform : GetComponent<RectTransform>();
			_draggable = _draggable != null ? _draggable : GetComponent<DraggableView>();
			_slices ??= GetComponentsInChildren<PieceSliceView>();
		}

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServicesLocator>();
			_dataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_canvasRaycaster = _canvasRaycaster != null ? _canvasRaycaster : GetComponentInParent<GraphicRaycaster>();
		}

		private void OnDestroy()
		{
			_dataProvider.PieceDataProvider.Pieces?.StopObserving(_uniqueId);
		}

		/// <inheritdoc />
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_draggable.enabled)
			{
				return;
			}

			if (!_canvasRaycaster.RaycastPoint(eventData.position, out var hits))
			{
				_draggable.ResetDraggable();
				return;
			}

			var hit = hits.Find(x => x.gameObject.HasComponent<TileViewController>());
			var tile = hit.gameObject?.GetComponent<TileViewController>();

			// Is not allowed to put a piece on a tile with already a piece in it
			if (!hit.isValid || _dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out _))
			{
				_draggable.ResetDraggable();
				return;
			}

			_draggable.enabled = false;

			MoveIntoTile(tile);
			_services.CommandService.ExecuteCommand(new PieceDropCommand(Id, tile.Row, tile.Column));
		}

		/// <inheritdoc />
		public void Init(IObjectPool<PieceViewController> pool)
		{
			_pool = pool;
		}

		/// <inheritdoc />
		public bool Despawn()
		{
			return _pool.Despawn(this);
		}

		/// <inheritdoc />
		public void OnSpawn(UniqueId id)
		{
			_uniqueId = id;
			_draggable.enabled = _dataProvider.GameplayBoardDataProvider.PieceDeck.Contains(id);

			_dataProvider.PieceDataProvider.Pieces.InvokeObserve(id, OnPieceUpdated);
		}

		/// <inheritdoc />
		public void OnDespawn()
		{
			_dataProvider.PieceDataProvider.Pieces.StopObserving(_uniqueId);

			_uniqueId = UniqueId.Invalid;
		}

		public void MoveIntoTile(TileViewController tile)
		{
			RectTransform.SetParent(tile.transform);
			RectTransform.SetAsLastSibling();

			RectTransform.anchoredPosition = Vector3.zero;
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
					Despawn();
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
	}
}

