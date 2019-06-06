using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class hwmAddressablesSystemUtility
{
	public static hwmAddressablesSystemConfig GetConfig()
	{
		string[] assetGuids = AssetDatabase.FindAssets("t:hwmAddressablesSystemConfig");
		if (assetGuids.Length > 0)
		{
			return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGuids[0])
				, typeof(hwmAddressablesSystemConfig)) as hwmAddressablesSystemConfig;
		}
		else
		{
			return null;
		}
	}

	[MenuItem("Custom/Addressables/Create Config")]
	private static hwmAddressablesSystemConfig CreateConfig()
	{
		hwmAddressablesSystemConfig config = GetConfig();
		if (config == null)
		{
			config = CreateAssetAtSelectionFolder<hwmAddressablesSystemConfig>("AddressablesSystemConfig");
		}

		return config;
	}

	[MenuItem("Custom/Addressables/Schema/Create Bundled Asset Group Schema")]
	private static BundledAssetGroupSchema CreateBundledAssetGroupSchema()
	{
		return CreateAssetAtSelectionFolder<BundledAssetGroupSchema>("BundledAssetGroupSchema");
	}

	[MenuItem("Custom/Addressables/Schema/Create Content Update Group Schema")]
	private static ContentUpdateGroupSchema CreateContentUpdateGroupSchema()
	{
		return CreateAssetAtSelectionFolder<ContentUpdateGroupSchema>("ContentUpdateGroupSchema");
	}

	[MenuItem("Custom/Addressables/Schema/Create Player Data Group Schema")]
	private static PlayerDataGroupSchema CreatePlayerDataGroupSchema()
	{
		return CreateAssetAtSelectionFolder<PlayerDataGroupSchema>("PlayerDataGroupSchema");
	}

	private static T CreateAssetAtSelectionFolder<T>(string assetName) where T : ScriptableObject
	{
		string selectionPath = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
		string configDirectory;
		if (Directory.Exists(selectionPath))
		{
			configDirectory = selectionPath;
		}
		else
		{
			if (File.Exists(selectionPath))
			{
				configDirectory = selectionPath.Substring(0, selectionPath.LastIndexOf('/'));
			}
			else
			{
				configDirectory = "Assets";
			}
		}

		string configPath = string.Format("{0}/{1}.asset", configDirectory, assetName);
		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), configPath);

		T asset = AssetDatabase.LoadAssetAtPath(configPath, typeof(T)) as T;
		if (asset)
		{
			Selection.activeObject = asset;
		}
		return asset;
	}
}