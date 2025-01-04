using UnityEngine;
using UnityEngine.UI;

namespace Game.ViewControllers
{
	public class PieceDeckViewController : ViewControllerBase
	{
		[SerializeField] private GraphicRaycaster _canvasRaycaster;
		
		public GraphicRaycaster CanvasRaycaster => _canvasRaycaster;

		protected override void OnEditorValidate()
		{
			_canvasRaycaster = _canvasRaycaster != null ? _canvasRaycaster : GetComponentInParent<GraphicRaycaster>();
		}
	}

}
