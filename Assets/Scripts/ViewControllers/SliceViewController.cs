using System;
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
	public class SliceViewController : ViewControllerBase, IPoolEntitySpawn<SliceColor>
	{
		private readonly Dictionary<SliceColor, Color> _colorMap = new()
		{
			{ SliceColor.White, Color.white },
			{ SliceColor.Black, Color.black },
			{ SliceColor.Red, Color.red },
			{ SliceColor.Yellow, Color.yellow },
			{ SliceColor.Green, Color.green },
			{ SliceColor.Blue, Color.blue },
			{ SliceColor.ColorCount, Color.clear },
		};

		[SerializeField] private Image _image;

		private SliceColor _sliceColor = SliceColor.ColorCount;

		public SliceColor SliceColor 
		{ 
			get => _sliceColor;
			set 
			{ 
				_sliceColor = value; 
				_image.color = _colorMap[_sliceColor];
			}
		}

		public bool IsDisabled => SliceColor == SliceColor.ColorCount;

		protected override void OnEditorValidate()
		{
			_image = _image == null ? GetComponent<Image>() : _image;
		}

		public void OnSpawn(SliceColor color)
		{
			SliceColor = color;
		}

		public void Disable()
		{
			SliceColor = SliceColor.ColorCount;
			
			gameObject.SetActive(false);
		}
	}
}