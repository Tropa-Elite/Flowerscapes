using System;
using Game.Data;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ViewControllers
{
	public class TileViewController : ViewControllerBase
	{
		[SerializeField] private Image _image;
		[SerializeField] private int _row = -1;
		[SerializeField] private int _column = -1;

		private Color _initialColor;

		public int Id => TileData.ToTileId(_row, _column);
		public int Row => _row;
		public int Column => _column;

		protected override void OnEditorValidate()
		{
			_image = _image == null ? GetComponent<Image>() : _image;
			
			if(Id >= 0)
			{
				return;
			}

			var splitName = gameObject.name.Split('_');

			_row = int.Parse(splitName[1]);
			_column = int.Parse(splitName[2]);
		}

		private void Awake()
		{
			_initialColor = _image.color;
		}

		public void SetOveringState(bool isOvering)
		{
			_image.color = isOvering ? Constants.Gameplay.TILE_OVERING_COLOR : _initialColor;
		}
	}
}
