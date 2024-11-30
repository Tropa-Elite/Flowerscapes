using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Views
{
	public class DraggableView : MonoBehaviour, IPointerDownHandler, IDragHandler
	{
		[SerializeField] private RectTransform _rectTransform;
		[HideInInspector]
		[SerializeField] private Transform _canvasTransform;

		private Vector2 _offset;
		private Vector2 _initialPosition;
		private Transform _initialParent;

		private void OnValidate()
		{
			_rectTransform = _rectTransform == null ? GetComponent<RectTransform>() : _rectTransform;
		}

		private void Start()
		{
			_canvasTransform = _canvasTransform == null ? GetComponentInParent<Canvas>().GetComponent<Transform>() : _canvasTransform;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_initialParent = _rectTransform.parent;

			_rectTransform.SetParent(_canvasTransform);
			_rectTransform.SetAsLastSibling();

			_offset = _rectTransform.anchoredPosition - eventData.position;
			_initialPosition = _rectTransform.anchoredPosition;
		}

		public void OnDrag(PointerEventData eventData)
		{
			_rectTransform.anchoredPosition = eventData.position + _offset;
		}

		public void ResetDraggable()
		{
			_rectTransform.anchoredPosition = _initialPosition;

			_rectTransform.SetParent(_initialParent);
		}
	}
}
