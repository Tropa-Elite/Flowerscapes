using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		[MenuItem("Tools/Scene/Force Compilation &r")]
		private static void ForceCompilation()
		{
			CompilationPipeline.RequestScriptCompilation();
		}

		[MenuItem("Tools/Scene/Open Game Scene &1")]
		private static void OpenGameScene()
		{
			EditorSceneManager.OpenScene(GetScenePath("Game"));
		}

		private static string GetScenePath(string scene)
		{
			return AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:scene {scene}")[0]);
		}

	}
}
