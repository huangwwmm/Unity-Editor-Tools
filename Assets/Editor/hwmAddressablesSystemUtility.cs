using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using static hwmAddressablesSystemConfig;

public static class hwmAddressablesSystemUtility
{
	#region Generate
	public static void GenerateAll(hwmAddressablesSystemConfig config)
	{
		AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
		for (int iGroupRule = 0; iGroupRule < config.GroupRules.Length; iGroupRule++)
		{
			GroupRule iterGroupRule = config.GroupRules[iGroupRule];
			if (string.IsNullOrWhiteSpace(iterGroupRule.GroupName))
			{
				Debug.LogError(string.Format("Group index ({0}) name is empty", iGroupRule));
				continue;
			}

			GenerateWithGroupRule(settings, config.MyGenerateSetting, iterGroupRule);
		}
		Debug.Log("Generate all group finish");
	}

	public static void GenerateSpecified(hwmAddressablesSystemConfig config)
	{
		string specifiedGroupName = config.MyGenerateSetting.SpecifiedGroupName;
		if (string.IsNullOrWhiteSpace(specifiedGroupName))
		{
			Debug.LogError("Specified group name is empty");
			return;
		}

		bool generate = false;
		AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
		for (int iGroupRule = 0; iGroupRule < config.GroupRules.Length; iGroupRule++)
		{
			GroupRule iterGroupRule = config.GroupRules[iGroupRule];
			if (iterGroupRule.GroupName == specifiedGroupName)
			{
				generate = true;
				GenerateWithGroupRule(settings, config.MyGenerateSetting, iterGroupRule);
			}
		}

		if (generate)
		{
			Debug.Log(string.Format("Generate specified group ({0}) finish", specifiedGroupName));
		}
		else
		{
			Debug.LogError(string.Format("Not found specified group ({0})", specifiedGroupName));
		}
	}

	private static void GenerateWithGroupRule(AddressableAssetSettings settings, GenerateSetting generateSetting, GroupRule groupRule)
	{
		AddressableAssetGroup oldGroup = settings.FindGroup(groupRule.GroupName);
		AddressableAssetGroup group;
		if (generateSetting.RecreateGroup)
		{
			if (oldGroup)
			{
				settings.RemoveGroup(oldGroup);
				oldGroup = null;
			}
			group = settings.CreateGroup(groupRule.GroupName, false, false, true, groupRule.SchemasToCopy);
		}
		else
		{
			if (oldGroup)
			{
				group = oldGroup;
			}
			else
			{
				group = settings.CreateGroup(groupRule.GroupName, false, false, true, groupRule.SchemasToCopy);
			}
		}

		for (int iAssetRule = 0; iAssetRule < groupRule.AssetRules.Length; iAssetRule++)
		{
			AssetRule iterAssetRule = groupRule.AssetRules[iAssetRule];
			GenerateWithAssetRule(settings, generateSetting, group, groupRule, iterAssetRule);
		}
	}

	private static void GenerateWithAssetRule(AddressableAssetSettings settings, GenerateSetting generateSetting, AddressableAssetGroup group, GroupRule groupRule, AssetRule assetRule)
	{
		int assetsIndexOf = Application.dataPath.LastIndexOf("Assets");
		string realPath = Application.dataPath.Substring(0, assetsIndexOf) + assetRule.Path;
		if (!Directory.Exists(realPath))
		{
			Debug.LogError(string.Format("Path ({0}) of group ({1}) not exists", realPath, groupRule.GroupName));
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(realPath);
		FileInfo[] files = directoryInfo.GetFiles("*.*", assetRule.IncludeChilder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		for (int iFile = 0; iFile < files.Length; iFile++)
		{
			FileInfo iterFile = files[iFile];
			string iterFileName = iterFile.Name.Substring(0, iterFile.Name.Length - iterFile.Extension.Length);
			// meta文件肯定是要忽略的
			if (iterFile.Extension == ".meta")
			{
				continue;
			}

			if (!Filter(assetRule.ExtensionFilterType, iterFile.Extension, assetRule.ExtensionFilters)
				|| !Filter(assetRule.FileNameFilterType, iterFileName, assetRule.FileNameFilters))
			{
				continue;
			}

			string iterAssetPath = iterFile.FullName.Substring(assetsIndexOf);
			string assetKey;
			switch (assetRule.AssetKeyType)
			{
				case AssetKeyType.FileName:
					assetKey = iterFileName;
					break;
				case AssetKeyType.FileNameFormat:
					assetKey = string.Format(assetRule.AssetKeyFormat, iterFileName);
					break;
				case AssetKeyType.Path:
					assetKey = iterAssetPath;
					break;
				default:
					assetKey = string.Empty;
					throw new System.Exception(string.Format("not support ExtensionFilterType ({0})", assetRule.ExtensionFilterType));
			}

			string iterAssetGuid = AssetDatabase.AssetPathToGUID(iterAssetPath);
			AddressableAssetEntry iterAssetEntry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(iterAssetGuid, group);
			if (iterAssetEntry == null)
			{
				Debug.LogError(string.Format("Cant load asset at path ({0})", iterAssetPath));
			}
			iterAssetEntry.SetAddress(assetKey);
			for (int iLabel = 0; iLabel < assetRule.AssetLables.Length; iLabel++)
			{
				iterAssetEntry.SetLabel(assetRule.AssetLables[iLabel], true);
			}
		}
	}

	private static bool Filter(FilterType filterType, string value, List<string> filters)
	{
		switch (filterType)
		{
			case FilterType.BlackList:
				return !filters.Contains(value);
			case FilterType.WhiteList:
				return filters.Contains(value);
			default:
				throw new System.Exception(string.Format("not support ExtensionFilterType ({0})", filterType));
		}
	}
	#endregion End Generate

	public static void CheckConfig(hwmAddressablesSystemConfig config)
	{
		for (int iGroup = 0; iGroup < config.GroupRules.Length; iGroup++)
		{
			GroupRule iterGroupRule = config.GroupRules[iGroup];
			if (string.IsNullOrWhiteSpace(iterGroupRule.GroupName))
			{
				Debug.LogError(string.Format("Group-{0}的GroupName为空", iterGroupRule.GroupName));
			}

			for (int iAsset = 0; iAsset < iterGroupRule.AssetRules.Length; iAsset++)
			{
				AssetRule iterAssetRule = iterGroupRule.AssetRules[iAsset];
				if (!(iterAssetRule.Path.StartsWith("Assets")
					&& iterAssetRule.Path.EndsWith("/")))
				{
					Debug.LogError(string.Format("Group-{0}({1})的AssetRule-{2}的Path不是\"Assets*/\"格式", iGroup, iterGroupRule.GroupName, iAsset));
				}

				for (int iExtension = 0; iExtension < iterAssetRule.ExtensionFilters.Count; iExtension++)
				{
					string iterExtension = iterAssetRule.ExtensionFilters[iExtension];
					if (!iterExtension.StartsWith("."))
					{
						Debug.LogError(string.Format("Group-{0}({1})的AssetRule-{2}的ExtensionFilters-{3}({4})不是\".*/\"格式", iGroup, iterGroupRule.GroupName, iAsset, iExtension, iterExtension));
					}
				}
			}
		}
		Debug.Log("Check config finish");
	}

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