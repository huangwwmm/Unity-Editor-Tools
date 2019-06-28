using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

public static class hwmCheckCompilePlayerScripts
{
	[MenuItem("Custom/Utility/Check Compile Player Scripts/Win64")]
	public static void CheckCompilePlayerScripts_Win64()
	{
		ScriptCompilationSettings settings = new ScriptCompilationSettings();
		settings.target = BuildTarget.StandaloneWindows64;
		settings.group = BuildTargetGroup.Standalone;
		settings.options = ScriptCompilationOptions.None;
		CheckCompilePlayerScripts(settings);
	}

	private static void CheckCompilePlayerScripts(ScriptCompilationSettings settings)
	{
		EditorUtility.DisplayProgressBar("Check Compile Player Scripts", "Do not operate the computer", 0);
		ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(settings, string.Format("{0}/Unity_{1}/", System.IO.Path.GetTempPath(), GUID.Generate()));
		EditorUtility.ClearProgressBar();

		if ((result.assemblies == null
				|| result.assemblies.Count == 0)
			&& result.typeDB == null)
		{
			EditorUtility.DisplayDialog("Check Compile Player Scripts", "Failed\nSee Console Window", "OK");
		}
		else
		{
			EditorUtility.DisplayDialog("Check Compile Player Scripts", "Success", "OK");
		}
	}
}