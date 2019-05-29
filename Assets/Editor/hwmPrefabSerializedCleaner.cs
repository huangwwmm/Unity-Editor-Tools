using UnityEditor;
using UnityEngine;

public class hwmPrefabSerializedCleaner : EditorWindow
{
	[MenuItem("Custom/Utility/Prefab Serialized Cleaner")]
	public static void Init()
	{
		GetWindow<hwmPrefabSerializedCleaner>("Prefab Serialized Cleaner");
	}

	protected void OnGUI()
	{
		if (GUILayout.Button("Clear all unused prefab serialized")
			&& EditorUtility.DisplayDialog("提示", "确定要执行这个操作？这个操作可能需要数分钟", "确认", "取消"))
		{
			int processPrefabCount = 0;
			string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
			for (int iGuid = 0; iGuid < prefabGuids.Length; iGuid++)
			{
				string iterPrefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[iGuid]);
				if (!iterPrefabPath.StartsWith("Assets"))
				{
					continue;
				}
				GameObject iterPrefab = AssetDatabase.LoadAssetAtPath(iterPrefabPath, typeof(GameObject)) as GameObject;
				if (iterPrefab != null)
				{
					GameObject go = Instantiate(iterPrefab);
					PrefabUtility.ReplacePrefab(go, iterPrefab);
					DestroyImmediate(go);
					processPrefabCount++;
				}
			}
			EditorUtility.DisplayDialog("提示", string.Format("处理了{0}个Prefab文件", processPrefabCount), "确认");
		}
	}
}