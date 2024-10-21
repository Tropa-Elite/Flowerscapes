using Game.Commands;
using Game.Ids;
using Game.Logic;
using Game.Services;
using Game.Utils;
using GameLovers.Services;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.MonoComponent
{
	public class PieceMonoComponent : MonoBehaviour, IDragHandler, IEndDragHandler, IPoolEntitySpawn<UniqueId>
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private GraphicRaycaster _raycaster;
		[SerializeField] private SlicePieceMonoComponent[] _slices;

		private IGameServices _services;
		private IGameDataProvider _dataProvider;
		private UniqueId _uniqueId;

		public RectTransform RectTransform => _rectTransform;
		public GraphicRaycaster Raycaster => _raycaster;
		public UniqueId Id => _uniqueId;

		void OnValidate()
		{
			_rectTransform ??= GetComponent<RectTransform>();
			_raycaster ??= GetComponentInParent<GraphicRaycaster>();
			_slices ??= GetComponentsInChildren<SlicePieceMonoComponent>();
		}

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();
			_dataProvider = MainInstaller.Resolve<IGameDataProvider>();
		}

		public void OnSpawn(UniqueId id)
		{
			var slices = _dataProvider.GameplayBoardDataProvider.Pieces[id].Slices;

			_uniqueId = id;

			for(var i = 0; i < Constants.Gameplay.MAX_PIECE_SLICES; i++)
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

		public void OnEndDrag(PointerEventData eventData)
		{
			_services.CommandService.ExecuteCommand(new PieceDropCommand { piece = this });
		}

		public void OnDrag(PointerEventData eventData)
		{
			// TODO: Important to keep
			//_rectTransform.anchoredPosition += eventData.delta * 3 * Constants.CANVAS_LOCAL_SCALE;
		}
	}
}

