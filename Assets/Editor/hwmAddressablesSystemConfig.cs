using System.IO;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor;

public class hwmAddressablesSystemConfig : ScriptableObject
{
	public GroupRule[] GroupRules;

	[ContextMenu("Test")]
	private void GenerateAll()
	{
		AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
		for (int iGroupRule = 0; iGroupRule < GroupRules.Length; iGroupRule++)
		{
			GroupRule iterGroupRule = GroupRules[iGroupRule];
			if (string.IsNullOrWhiteSpace(iterGroupRule.GroupName))
			{
				Debug.LogError(string.Format("Group index ({0}) name is empty", iGroupRule));
				continue;
			}

			GenerateWithGroupRule(settings, iterGroupRule);
		}
	}

	private void GenerateWithGroupRule(AddressableAssetSettings settings, GroupRule groupRule)
	{
		AddressableAssetGroup oldGroup = settings.FindGroup(groupRule.GroupName);
		if (oldGroup)
		{
			settings.RemoveGroup(oldGroup);
			oldGroup = null;
		}

		AddressableAssetGroup group = settings.CreateGroup(groupRule.GroupName, false, false, true, groupRule.SchemasToCopy);
		for (int iAssetRule = 0; iAssetRule < groupRule.AssetRules.Length; iAssetRule++)
		{
			AssetRule iterAssetRule = groupRule.AssetRules[iAssetRule];
			GenerateWithAssetRule(settings, group, groupRule, iterAssetRule);
		}
	}

	private void GenerateWithAssetRule(AddressableAssetSettings settings, AddressableAssetGroup group, GroupRule groupRule, AssetRule assetRule)
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

	private bool Filter(FilterType filterType, string value, List<string> filters)
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

	[System.Serializable]
	public struct GroupRule
	{
		public string GroupName;
		public List<AddressableAssetGroupSchema> SchemasToCopy;
		public AssetRule[] AssetRules;
	}

	[System.Serializable]
	public struct AssetRule
	{
		/// <summary>
		/// 路径
		/// </summary>
		public string Path;
		/// <summary>
		/// 包含子目录
		/// </summary>
		public bool IncludeChilder;
		/// <summary>
		/// <see cref="AddressableAssetEntry.address"/>
		/// </summary>
		public AssetKeyType AssetKeyType;
		/// <summary>
		/// <see cref="AssetKeyType.FileNameFormat"/>时用的
		/// </summary>
		public string AssetKeyFormat;
		/// <summary>
		/// 决定<see cref="ExtensionFilters"/>的用途
		/// </summary>
		public FilterType ExtensionFilterType;
		/// <summary>
		/// <see cref="ExtensionFilterType"/>
		/// </summary>
		public List<string> ExtensionFilters;
		/// <summary>
		/// 决定<see cref="FileNameFilters"/>的用途
		/// </summary>
		public FilterType FileNameFilterType;
		/// <summary>
		/// <see cref="FileNameFilterType"/>
		/// </summary>
		public List<string> FileNameFilters;
		/// <summary>
		/// <see cref="AddressableAssetEntry.labels"/>
		/// </summary>
		public string[] AssetLables;
	}

	/// <summary>
	/// 扩展名筛选的类型
	/// </summary>
	public enum FilterType
	{
		/// <summary>
		/// 白名单
		/// </summary>
		WhiteList,
		/// <summary>
		/// 黑名单
		/// </summary>
		BlackList,
	}

	/// <summary>
	/// <see cref="AddressableAssetEntry.address"/>的类型
	/// </summary>
	public enum AssetKeyType
	{
		/// <summary>
		/// 完整路径(Assets/xxx)
		/// </summary>
		Path,
		/// <summary>
		/// 文件名，可能会出现重复
		/// </summary>
		FileName,
		/// <summary>
		/// FileName的基础上<see cref="string.Format"/>
		/// </summary>
		FileNameFormat,
	}
}