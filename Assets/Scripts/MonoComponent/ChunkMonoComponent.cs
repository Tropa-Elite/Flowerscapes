using UnityEngine;
using UnityEngine.UI;

namespace Game.MonoComponent
{
	[RequireComponent(typeof(Image))]
	public class ChunkMonoComponent : MonoBehaviour
	{
		[SerializeField] private Image _image;

		public Color Color 
		{ 
			get { return _image.color; }
			set { _image.color = value; }
		}

		private void OnValidate()
		{
			_image = _image != null ? _image : GetComponent<Image>();
		}

		public void GenerateRandomColor()
		{
			Color = Constants.AVAILABLE_COLORS[Random.Range(0, Constants.NUMBER_OF_COLORS)];
		}
	}
}