using Game.Data;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
	[RequireComponent(typeof(Image))]
	public class PieceSliceView : MonoBehaviour
	{
		private readonly Dictionary<SliceColor, Color> _colorMap = new Dictionary<SliceColor, Color>
		{
			{ SliceColor.White, Color.white },
			{ SliceColor.Black, Color.black },
			{ SliceColor.Red, Color.red },
			{ SliceColor.Yellow, Color.yellow },
			{ SliceColor.Green, Color.green },
			{ SliceColor.Blue, Color.blue },
		};

		[SerializeField] private Image _image;

		private SliceColor _sliceColor;

		public SliceColor SliceColor 
		{ 
			get { return _sliceColor; }
			set 
			{ 
				_sliceColor = value; 
				_image.color = _colorMap[_sliceColor];
			}
		}

		private void OnValidate()
		{
			_image ??= GetComponent<Image>();
		}
	}
}