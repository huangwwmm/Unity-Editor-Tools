using System.IO;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor;

public class hwmAddressablesSystemConfig : ScriptableObject
{
	[hwmAddressablesSystemEditor()]
	public bool _ForInspector;

	public GenerateSetting MyGenerateSetting;
	[Space(10)]
	public GroupRule[] GroupRules;

	[System.Serializable]
	public struct GenerateSetting
	{
		/// <summary>
		/// true：如果Group已经存在，会先移除Group再重新创建
		/// 这个过程会比较耗时，如果没有严重BUG，不要勾选这个选项
		/// </summary>
		[Tooltip("true：如果Group已经存在，会先移除Group再重新创建\n这个过程会比较耗时，如果没有严重BUG，不要勾选这个选项")]
		public bool RecreateGroup;
		/// <summary>
		/// false时不应用AssetRule
		/// 这个选项的目的是应用AssetRule比较耗时，有时只想移除无效Asset，不勾选这个选项可以加快速度
		/// </summary>
		[Tooltip("false时不应用AssetRule\n这个选项的目的是应用AssetRule比较耗时，有时只想移除无效Asset，不勾选这个选项可以加快速度")]
		public bool ApplyAssetRule;
		/// <summary>
		/// 移除无效的Asset，包括：
		///		文件Missing
		/// </summary>
		[Tooltip("移除无效的Asset，包括：\n\t文件Missing")]
		public bool RemoveInvalidAsset;
		/// <summary>
		/// Generate Specified的组
		/// </summary>
		[Tooltip("Generate Specified的组")]
		public string SpecifiedGroupName;
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
		/// 路径，以Assets开头，结尾要加/
		/// </summary>
		[Tooltip("路径，以Assets开头，结尾要加/")]
		public string Path;
		/// <summary>
		/// true: 递归所有子目录 false：只查找顶目录
		/// </summary>
		[Tooltip("true: 递归所有子目录 false：只查找顶目录")]
		public bool IncludeChilder;
		/// <summary>
		/// <see cref="AddressableAssetEntry.address"/>
		/// </summary>
		[Tooltip("生成的Asset的Key的规则")]
		public AssetKeyType AssetKeyType;
		/// <summary>
		/// <see cref="AssetKeyType.FileNameFormat"/>时用的
		/// string.Format(AssetKeyFormat, assetFileName)
		/// 例如Asset文件名为abc，AssetKeyFormat为M_{0}，最终生成的AssetKey为M_abc
		/// </summary>
		[Tooltip("AssetKeyType为FileNameFormat时用的，string.Format(AssetKeyFormat, assetFileName)\n例如Asset文件名为abc，AssetKeyFormat为M_{0}，最终生成的AssetKey为M_abc")]
		public string AssetKeyFormat;
		/// <summary>
		/// 决定<see cref="ExtensionFilters"/>的用途
		/// </summary>
		[Tooltip("扩展名的筛选规则")]
		public FilterType ExtensionFilterType;
		/// <summary>
		/// <see cref="ExtensionFilterType"/>
		/// 开头必须是"."
		/// </summary>
		[Tooltip("开头必须是\".\"")]
		public List<string> ExtensionFilters;
		/// <summary>
		/// 决定<see cref="FileNameFilters"/>的用途
		/// </summary>
		[Tooltip("文件名的筛选规则")]
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