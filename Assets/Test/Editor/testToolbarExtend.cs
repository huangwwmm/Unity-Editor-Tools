using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class testToolbarExtend
{
	static testToolbarExtend()
	{
		hwmToolbarExtend.OnLeftToolbarGUI += OnLeftGUI;
		hwmToolbarExtend.OnRightToolbarGUI += OnRightGUI;
	}

	private static void OnLeftGUI()
	{
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(new GUIContent("G", "LLLL"), hwmToolbarExtend.GetCommandButtonStyle()))
		{
			Debug.LogError("LLLL");
		}
	}


	private static void OnRightGUI()
	{
		if (GUILayout.Button(new GUIContent("■", "RRRR"), hwmToolbarExtend.GetCommandButtonStyle()))
		{
			Debug.LogError("RRRR");
		}
	}
}