using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.MonoComponent
{
	public class TileMonoComponent : MonoBehaviour
	{
		[SerializeField] private int _row = -1;
		[SerializeField] private int _column = -1;

		public int Row => _row;
		public int Column => _column;

		private void OnValidate()
		{
			if(_row >= 0)
			{
				return;
			}

			var name = gameObject.name.Split('_');

			_row = int.Parse(name[1]);
			_column = int.Parse(name[2]);
		}
	}
}
