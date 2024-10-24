using Game.Commands;
using Game.Ids;
using Game.Logic;
using Game.Services;
using Game.Utils;
using GameLovers;
using GameLovers.Services;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.MonoComponent
{
	public class PieceMonoComponent : MonoBehaviour, IPoolEntitySpawn<UniqueId>, IPointerUpHandler
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private DraggableMonoComponent _draggable;
		[SerializeField] private SlicePieceMonoComponent[] _slices;
		//[HideInInspector]
		[SerializeField] private GraphicRaycaster _canvasRaycaster;

		private IGameServices _services;
		private IGameDataProvider _dataProvider;
		private UniqueId _uniqueId;

		public RectTransform RectTransform => _rectTransform;
		public UniqueId Id => _uniqueId;

		void OnValidate()
		{
			_rectTransform ??= GetComponent<RectTransform>();
			_draggable ??= GetComponent<DraggableMonoComponent>();
			_slices ??= GetComponentsInChildren<SlicePieceMonoComponent>();
		}

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();
			_dataProvider = MainInstaller.Resolve<IGameDataProvider>();
			_canvasRaycaster ??= GetComponentInParent<GraphicRaycaster>();
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

			var hit = hits.Find(x => x.gameObject.HasComponent<TileMonoComponent>());
			var tile = hit.gameObject?.GetComponent<TileMonoComponent>();

			// Is not allowed to put a piece on a tile with already a piece in it
			if (!hit.isValid || _dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out _))
			{
				_draggable.ResetDraggable();
				return;
			}

			_draggable.enabled = false;

			tile.SetPiece(this);
			_services.CommandService.ExecuteCommand(new PieceDropCommand(Id, tile.Row, tile.Column));
		}

		public void OnSpawn(UniqueId id)
		{
			var slices = _dataProvider.GameplayBoardDataProvider.Pieces[id].Slices;

			_uniqueId = id;
			_draggable.enabled = _dataProvider.GameplayBoardDataProvider.PieceDeck.Contains(id);

			for (var i = 0; i < Constants.Gameplay.MAX_PIECE_SLICES; i++)
			{
				if(i >= slices.Count)
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

