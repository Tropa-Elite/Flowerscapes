﻿using DG.Tweening;
using Game.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.ViewControllers
{
	public class DraggableViewController : ViewControllerBase, IPointerDownHandler, IDragHandler
	{
		public float DragSpeed = 1f;
		public bool TweenPivot = true;
		public Vector2 PivotOffset = Vector2.up;
		
		private Tweener _resetTweener;
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

		// Order of execution is important in this method because parenting changes the local reference position of the transform
		public void OnPointerDown(PointerEventData eventData)
		{
			if (_resetTweener.IsActive() && _resetTweener.IsPlaying()) return;
			
			_initialParent = RectTransform.parent;
			_initialPosition = RectTransform.anchoredPosition;
			
			RectTransform.SetParent(_canvasTransform);
			RectTransform.SetAsLastSibling();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform,
				eventData.position, null, out var position);
			
			RectTransform.pivot = _initialPivot - PivotOffset;
			RectTransform.anchoredPosition = position;
			_previousPosition = RectTransform.anchoredPosition;
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

		public void ResetPivot()
		{
			RectTransform.pivot = _initialPivot;
		}

		public void ResetPosition()
		{
			RectTransform.SetParent(_initialParent);
			ResetPivot();
			
			if (TweenPivot)
			{
				var duration = Constants.Gameplay.Piece_Pivot_Tween_Time;
				
				_resetTweener = DOVirtual.Vector2(RectTransform.anchoredPosition, _initialPosition, duration, UpdatePosition);
			}
			else
			{
				RectTransform.anchoredPosition = _initialPosition;
			}
		}

		private void UpdatePosition(Vector2 value)
		{
			RectTransform.anchoredPosition = value;
		}
	}
}
