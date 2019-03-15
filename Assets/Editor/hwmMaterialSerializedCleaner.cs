using UnityEditor;
using UnityEngine;

public class hwmMaterialSerializedCleaner : EditorWindow
{
	[MenuItem("Custom/Utility/Material Serialized Cleaner")]
	public static void Init()
	{
		GetWindow<hwmMaterialSerializedCleaner>("Material Serialized Cleaner");
	}

	protected void OnGUI()
	{
		if (GUILayout.Button("Clear all unused material serialized")
			&& EditorUtility.DisplayDialog("提示", "确定要执行这个操作？这个操作可能需要数分钟", "确认", "取消"))
		{
			int processMaterialCount = 0;
			string[] materialGuids = AssetDatabase.FindAssets("t:Material");
			for (int iGuid = 0; iGuid < materialGuids.Length; iGuid++)
			{
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
			EditorUtility.DisplayDialog("提示", string.Format("处理了{0}个Material文件", processMaterialCount), "确认");
		}
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