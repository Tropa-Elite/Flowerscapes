using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEditor.Compilation;

namespace Game.Editor
{
	/// <summary>
	/// This editor class helps creating Unity editor shortcuts
	/// </summary>
	public class EditorShortcuts
	{
		[MenuItem("Tools/Scene/Force Script Reload &r")]
		private static void ForceScriptReload()
		{
			//CompilationPipeline.RequestScriptCompilation();
			EditorUtility.RequestScriptReload();
		}

		[MenuItem("Tools/Scene/Open Boot Scene &1")]
		private static void OpenBootScene()
		{
			EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
		}

		[MenuItem("Tools/Scene/Open Main Scene &2")]
		private static void OpenMainScene()
		{
			EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
		}

		[MenuItem("Tools/Scene/Open First Scene &3")]
		private static void OpenFirstScene()
		{
			EditorSceneManager.OpenScene(GetScenePath());
		}

		private static string GetScenePath(string scene = "")
		{
			return AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:scene {scene}")[0]);
		}

	}
}
