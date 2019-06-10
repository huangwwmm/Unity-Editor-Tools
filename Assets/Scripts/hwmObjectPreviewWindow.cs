#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

public class hwmObjectPreviewWindow : EditorWindow
{
	private object m_Target;

	private float m_LabelWidth = 240.0f;
	private Vector3 m_ScrollPosition;

	private Dictionary<string, bool> m_Foldouts = new Dictionary<string, bool>();

	[MenuItem("Custom/Utility/Test Object Data Preview")]
	public static void ShowTestWindow()
	{
		TestTarget ttt = new TestTarget();
		ttt.Ass = new int[] { 5, 8, 6, 3, 5 };
		ttt.Defd.Add("dasfsdf");
		ttt.Defd.Add("dasfsdf3");
		ttt.Defd.Add("dasfsdf6");
		ttt.Defd.Add("dasfsdf7");
		List<TestTarget> t1 = new List<TestTarget>();
		t1.Add(new TestTarget());
		t1.Add(new TestTarget());
		List<TestTarget> t2 = new List<TestTarget>();
		t2.Add(new TestTarget());
		t2.Add(new TestTarget());
		ttt.FE.Add(t1);
		ttt.FE.Add(t2);

		GetWindow<hwmObjectPreviewWindow>("Object Data Preview", true).Initialize(ttt);
	}

	public static void ShowWindow(object target)
	{
		GetWindow<hwmObjectPreviewWindow>("Object Data Preview", true).Initialize(target);
	}

	protected void Initialize(object target)
	{
		m_Target = target;
	}

	protected void OnGUI()
	{
		m_LabelWidth = EditorGUILayout.DelayedFloatField("LabelWidth", m_LabelWidth);
		EditorGUIUtility.labelWidth = m_LabelWidth;
		EditorGUILayout.Space();

		EditorGUI.indentLevel = 0;
		m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUI.skin.scrollView);
		OnGUIObject(m_Target);
		EditorGUILayout.EndScrollView();
	}

	private void OnDestroy()
	{
		m_Target = null;
	}

	private void OnGUIObject(object graph, string fieldDisplayName = "Root", string foldoutName = "")
	{
		if (graph == null)
		{
			EditorGUILayout.LabelField(fieldDisplayName, "Null");
			return;
		}

		bool foldout;
		bool indentLevel;
		if (string.IsNullOrEmpty(foldoutName))
		{
			foldout = true;
			indentLevel = false;
		}
		else
		{
			foldoutName = foldoutName + fieldDisplayName;
			foldout = SetFoldout(foldoutName, EditorGUILayout.Foldout(GetFoldout(foldoutName), fieldDisplayName));
			indentLevel = true;
		}

		if (foldout)
		{
			EditorGUI.indentLevel += indentLevel ? 1 : 0;

			Type graphType = graph.GetType();
			FieldInfo[] fieldInfos = graphType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			for (int fieldIdx = 0; fieldIdx < fieldInfos.Length; fieldIdx++)
			{
				FieldInfo iterFieldInfo = fieldInfos[fieldIdx];
				string iterFieldDisplayName = iterFieldInfo.Name;
				OnGUIField(iterFieldInfo.FieldType, iterFieldDisplayName, foldoutName, iterFieldInfo.GetValue(graph));
			}

			EditorGUI.indentLevel -= indentLevel ? 1 : 0;
		}
	}

	private void OnGUIField(Type fieldType, string fieldDisplayName, string foldoutName, object value)
	{
		if (fieldType.IsValueType || fieldType == typeof(string))
		{
			OnGUIValue(fieldType, fieldDisplayName, value);
		}
		else if (fieldType.IsGenericType)
		{
			OnGUIList(fieldType, fieldType.GetGenericArguments()[0], fieldDisplayName, foldoutName, value);
		}
		else if (fieldType.IsArray)
		{
			OnGUIList(fieldType, fieldType.GetElementType(), fieldDisplayName, foldoutName, value);
		}
		else if (fieldType.IsClass)
		{
			if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				EditorGUILayout.ObjectField(fieldDisplayName, value as UnityEngine.Object, fieldType, true);
			}
			else
			{
				OnGUIObject(value, fieldDisplayName, foldoutName);
			}
		}
		else
		{
			EditorGUILayout.LabelField("Error Field: " + fieldType.ToString());
		}
	}

	/// <summary>
	/// string虽然是class，但是string结构很简单所以把它当成value处理
	/// </summary>
	/// <param name="type"></param>
	/// <param name="fieldDisplayName"></param>
	/// <param name="oldValue"></param>
	private void OnGUIValue(Type type, string fieldDisplayName, object oldValue)
	{
		if (type == typeof(int))
		{
			EditorGUILayout.IntField(fieldDisplayName + "(I)", (int)oldValue);
		}
		else if (type == typeof(string))
		{
			EditorGUILayout.TextField(fieldDisplayName + "(S)", (string)oldValue);
		}
		else if (type == typeof(float))
		{
			EditorGUILayout.FloatField(fieldDisplayName + "(F)", (float)oldValue);
		}
		else if (type == typeof(double))
		{
			EditorGUILayout.DoubleField(fieldDisplayName + "(D)", (double)oldValue);
		}
		else if (type == typeof(bool))
		{
			EditorGUILayout.Toggle(fieldDisplayName + "(B)", (bool)oldValue);
		}
		else
		{
			EditorGUILayout.LabelField(fieldDisplayName, string.Format("Not support type ({0})" + type.ToString()));
		}
	}

	/// <summary>
	/// 继承自<see cref="IList"/>的都可以
	/// </summary>
	private void OnGUIList(Type fieldType, Type elementType, string fieldDisplayName, string foldoutName, object value)
	{
		if (value == null)
		{
			EditorGUILayout.LabelField(fieldDisplayName, "Null");
			return;
		}

		foldoutName = foldoutName + fieldDisplayName;

		IList objectList = (IList)value;

		// Foldout
		if (SetFoldout(foldoutName, EditorGUILayout.Foldout(GetFoldout(foldoutName), string.Format("{0} Count:{1}", fieldDisplayName, objectList.Count))))
		{
			EditorGUI.indentLevel++;
			// Elements
			for (int iObj = 0; iObj < objectList.Count; iObj++)
			{
				object iterObj = objectList[iObj];
				string iterFieldDisplayName = "Elements " + iObj;
				string iterFoldoutName = foldoutName + iterFieldDisplayName;
				OnGUIField(elementType, iterFieldDisplayName, iterFoldoutName, iterObj);
			}
			EditorGUI.indentLevel--;
		}
	}

	private bool GetFoldout(string foldoutName, bool defaultFoldout = false)
	{
		bool foldout = defaultFoldout;
		m_Foldouts.TryGetValue(foldoutName, out foldout);
		return foldout;
	}

	private bool SetFoldout(string foldoutName, bool foldout)
	{
		m_Foldouts[foldoutName] = foldout;
		return foldout;
	}

	public class TestTarget
	{
		public int[] Ass = new int[5];
		public List<string> Defd = new List<string>();
		public List<List<TestTarget>> FE = new List<List<TestTarget>>();
		public GameObject go;
		public Animation an;
	}
}
#endif