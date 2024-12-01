using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Game.ViewControllers
{
	public class DraggableViewController : ViewControllerBase, IPointerDownHandler, IDragHandler
	{
		public float DragSpeed = 1f;
		public Vector2 Offset = Vector2.up;
		
		private Vector2 _initialPivot;
		private Vector2 _initialPosition;
		private Vector2 _previousPosition;
		private Transform _initialParent;
		private RectTransform _canvasTransform;

		private void Awake()
		{
			_canvasTransform = _canvasTransform == null ? GetComponentInParent<Canvas>().GetComponent<RectTransform>() : _canvasTransform;
			_initialPivot = RectTransform.pivot;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_initialParent = RectTransform.parent;

			RectTransform.SetParent(_canvasTransform);
			RectTransform.SetAsLastSibling();
			
			RectTransform.pivot = _initialPivot - Offset;
			_initialPosition = RectTransform.anchoredPosition;
			_previousPosition = _initialPosition;
		}

		public void OnDrag(PointerEventData eventData)
		{
			// Convert the screen space into canvas space
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform,
				    eventData.position, null, out var position))
			{
				RectTransform.anchoredPosition += (position - _previousPosition) * DragSpeed;
				_previousPosition = position;
			}
		}

		public void ResetDraggable()
		{
			RectTransform.pivot = _initialPivot;
			RectTransform.anchoredPosition = _initialPosition;
			
			RectTransform.SetParent(_initialParent);
		}

		public void MoveIntoTransform(Transform newTransform)
		{
			RectTransform.SetParent(newTransform);
			RectTransform.SetAsLastSibling();

			RectTransform.pivot = _initialPivot;
			RectTransform.anchoredPosition = Vector3.zero;
		}
	}
}
