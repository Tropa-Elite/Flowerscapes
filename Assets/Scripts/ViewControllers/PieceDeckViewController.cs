using UnityEngine;

namespace Game.ViewControllers
{
	public class PieceDeckViewController : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform;

		public RectTransform RectTransform => _rectTransform;

		private void OnValidate()
		{
			_rectTransform = _rectTransform == null ? GetComponent<RectTransform>() : _rectTransform;
		}
	}

}
