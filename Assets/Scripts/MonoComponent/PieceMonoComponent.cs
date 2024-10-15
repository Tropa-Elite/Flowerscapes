using Game.Commands;
using Game.Ids;
using Game.Services;
using GameLovers.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.MonoComponent
{
	public class PieceMonoComponent : MonoBehaviour, IDragHandler, IEndDragHandler, IPoolEntitySpawn<UniqueId>
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private GraphicRaycaster _raycaster;
		[SerializeField] private ChunkMonoComponent[] _chunks;

		private IGameServices _services;
		private UniqueId _uniqueId;

		public RectTransform RectTransform => _rectTransform;
		public GraphicRaycaster Raycaster => _raycaster;
		public UniqueId Id => _uniqueId;

		void OnValidate()
		{
			_rectTransform ??= GetComponent<RectTransform>();
			_raycaster ??= GetComponentInParent<GraphicRaycaster>();
			_chunks ??= GetComponentsInChildren<ChunkMonoComponent>();
		}

		private void Awake()
		{
			_services = MainInstaller.Resolve<IGameServices>();
		}

		public void OnSpawn(UniqueId id)
		{
			_uniqueId = id;

			gameObject.SetActive(true);
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

