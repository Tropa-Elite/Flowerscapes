using Game.Ids;
using GameLoversEditor.UiService;
using UnityEditor;

namespace Game.Editor
{
	/// <summary>
	/// Games custom <see cref="UiConfigsEditor{TSet}"/>
	/// </summary>
	[CustomEditor(typeof(GameLovers.UiService.UiConfigs))]
	public class GameUiConfigsEditor : UiConfigsEditor<UiSetId>
	{
	}
}
