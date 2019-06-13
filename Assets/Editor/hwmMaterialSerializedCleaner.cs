using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class hwmMaterialSerializedCleaner : EditorWindow
{
	private const string TOOL_NAME = "Material Serialized Cleaner";
	private const long UPDATE_PROGRESSBAR_MILLISECONDS_INTERVAL = 500;

	[MenuItem("Custom/Utility/" + TOOL_NAME)]
	public static void Init()
	{
		GetWindow<hwmMaterialSerializedCleaner>(TOOL_NAME);
	}

	protected void OnGUI()
	{
		if (GUILayout.Button("Clear all unused material serialized")
			&& EditorUtility.DisplayDialog("提示", "确定要执行这个操作？这个操作可能需要数分钟", "确认", "取消"))
		{
			hwmEditorCoroutine.Start(ClearAllUnusedMaterialSerialized());
		}
	}

	private IEnumerator ClearAllUnusedMaterialSerialized()
	{
		int processMaterialCount = 0;
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		long lastProgressBarMilliseconds = 0;
		string[] materialGuids = AssetDatabase.FindAssets("t:Material");
		for (int iGuid = 0; iGuid < materialGuids.Length; iGuid++)
		{
			if (stopwatch.ElapsedMilliseconds - lastProgressBarMilliseconds > UPDATE_PROGRESSBAR_MILLISECONDS_INTERVAL)
			{
				lastProgressBarMilliseconds = stopwatch.ElapsedMilliseconds;
				if (UpdateCancelableProgressBar(string.Format("Cleaning up {0}/{1}", iGuid, materialGuids.Length), (float)iGuid / materialGuids.Length))
				{
					yield break;
				}
				AssetDatabase.SaveAssets();
			}

			Material iterMaterial = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(materialGuids[iGuid]), typeof(Material)) as Material;
			if (iterMaterial != null)
			{
				SerializedObject iterSerializedObject = new SerializedObject(iterMaterial);
				bool hasUnused = false;
				hasUnused |= ProcessProperties(iterMaterial, iterSerializedObject, "m_SavedProperties.m_TexEnvs");
				hasUnused |= ProcessProperties(iterMaterial, iterSerializedObject, "m_SavedProperties.m_Floats");
				hasUnused |= ProcessProperties(iterMaterial, iterSerializedObject, "m_SavedProperties.m_Colors");
				iterSerializedObject.ApplyModifiedProperties();
				processMaterialCount += hasUnused ? 1 : 0;
			}
		}
		yield return null;
		EditorUtility.ClearProgressBar();
		stopwatch.Stop();
		EditorUtility.DisplayDialog(TOOL_NAME, string.Format("Cleaned up {0} material in {1} ms", processMaterialCount, stopwatch.ElapsedMilliseconds), "确认");
	}

	private bool UpdateCancelableProgressBar(string text, float progress)
	{
		return EditorUtility.DisplayCancelableProgressBar(TOOL_NAME, text, progress);
	}

	private bool ProcessProperties(Material material, SerializedObject serializedObject, string path)
	{
		var properties = serializedObject.FindProperty(path);
		if (properties == null || !properties.isArray)
		{
			return false;
		}

		bool hasUnused = false;
		for (int iProperty = properties.arraySize - 1; iProperty >= 0; iProperty--)
		{
			string iterPropertyName = properties.GetArrayElementAtIndex(iProperty).displayName;

			if (!material.HasProperty(iterPropertyName))
			{
				properties.DeleteArrayElementAtIndex(iProperty);
				hasUnused = true;
			}
		}
		return hasUnused;
	}
}