using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class hwmButtonAttribute : PropertyAttribute
{
	/// <summary>
	/// 按钮上显示的文本
	/// </summary>
	public readonly string Text;
	/// <summary>
	/// 点击按钮的回调
	/// </summary>
	public readonly string CallbackMethodName;
	public readonly System.Reflection.BindingFlags CallbackMethodBindingFlags;
	public readonly string CanDisplayMethodName;
	public readonly System.Reflection.BindingFlags CanDisplayMethodBindingFlags;

	public hwmButtonAttribute(string text
		, string callbackMethodName
		, System.Reflection.BindingFlags callbackMethodBindingFlags)
	{
		Text = text;
		CallbackMethodName = callbackMethodName;
		CallbackMethodBindingFlags = callbackMethodBindingFlags;
	}

	public hwmButtonAttribute(string text
		, string callbackMethodName
		, System.Reflection.BindingFlags callbackMethodBindingFlags
		, string canDisplayMethodName
		, System.Reflection.BindingFlags canDisplayMethodBindingFlags)
		: this(text, callbackMethodName, callbackMethodBindingFlags)
	{
		CanDisplayMethodName = canDisplayMethodName;
		CanDisplayMethodBindingFlags = canDisplayMethodBindingFlags;
	}
}
