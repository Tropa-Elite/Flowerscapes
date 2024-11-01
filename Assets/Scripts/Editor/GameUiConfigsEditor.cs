using Game.Ids;
using GameLoversEditor.UiService;
using UnityEditor;

namespace Game.Editor
{
	[CustomEditor(typeof(GameLovers.UiService.UiConfigs))]
	public class GameUiConfigsEditor : UiConfigsEditor<UiSetId>
	{
	}
}
