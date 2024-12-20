using Game.Data;
using Game.Utils;
using System.Collections.Generic;
using DG.Tweening;
using GameLovers.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ViewControllers
{
	[RequireComponent(typeof(Image))]
	public class SliceViewController : ViewControllerBase, IPoolEntitySpawn<SliceColor>, IPoolEntityDespawn
	{
		private readonly Dictionary<SliceColor, Color> _colorMap = new()
		{
			{ SliceColor.White, Color.white },
			{ SliceColor.Black, Color.black },
			{ SliceColor.Red, Color.red },
			{ SliceColor.Yellow, Color.yellow },
			{ SliceColor.Green, Color.green },
			{ SliceColor.Blue, Color.blue },
			{ SliceColor.ColorCount, Color.magenta },
		};

		[SerializeField] private Image _image;

		private SliceColor _sliceColor = SliceColor.ColorCount;
		private Tweener _rotateTweener;

		public SliceColor SliceColor 
		{ 
			get => _sliceColor;
			set 
			{ 
				_sliceColor = value; 
				_image.color = _colorMap[_sliceColor];
			}
		}

		protected override void OnEditorValidate()
		{
			_image = _image == null ? GetComponent<Image>() : _image;
		}

		public void OnSpawn(SliceColor color)
		{
			SliceColor = color;
		}

		public void OnDespawn()
		{
			RectTransform.DOKill();
		}

		public void StartTransferAnimation(PieceViewController fromPiece, PieceViewController toPiece, int sliceIndex, float delay)
		{
			var duration = Constants.Gameplay.Slice_Transfer_Tween_Time;
			var indexClosure = sliceIndex;
			var toPieceClosure = toPiece;
			var finalRotation = sliceIndex * Constants.Gameplay.Slice_Rotation;
			
			_rotateTweener?.Kill();
			
			_rotateTweener = RectTransform.DORotate(finalRotation, duration * 0.95f).SetDelay(delay);
			
			RectTransform.DOMove(toPieceClosure.RectTransform.position, duration) 
				.SetDelay(delay)
				.OnComplete(() => toPieceClosure.AddSlice(indexClosure, this));
		}

		public void StartRotateAnimation(Vector3 previousSliceRotation)
		{
			var duration = Constants.Gameplay.Slice_Rotation_Tween_Time;
			var rotation = previousSliceRotation + Constants.Gameplay.Slice_Rotation;

			if (Freya.Mathfs.DistanceSquared(RectTransform.rotation.eulerAngles, rotation) < 0.0001f) return;
			
			_rotateTweener?.Kill();

			_rotateTweener = RectTransform.DORotate(rotation, duration);
		}
	}
}