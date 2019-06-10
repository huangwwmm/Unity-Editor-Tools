using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(hwmButtonAttribute))]
public sealed class hwmButtonDrawer : PropertyDrawer
{
	private bool m_IsInitialized = false;
	private bool m_IsVaild;
	private Type m_ObjectType;
	private MethodInfo m_CallbackMethod;
	private MethodInfo m_CanDisplayMethod;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		hwmButtonAttribute attribute = this.attribute as hwmButtonAttribute;
		if (!m_IsInitialized)
		{
			m_IsInitialized = true;

			m_IsVaild = !string.IsNullOrWhiteSpace(attribute.Text)
				&& !string.IsNullOrWhiteSpace(attribute.CallbackMethodName);

			if (m_IsVaild)
			{
				m_ObjectType = property.serializedObject.targetObject.GetType();
				if (m_ObjectType == null)
				{
					return;
				}
				m_CallbackMethod = m_ObjectType.GetMethod(attribute.CallbackMethodName, attribute.CallbackMethodBindingFlags);
				if (m_CallbackMethod == null)
				{
					Debug.LogError(string.Format("not found method({0}) in type({1})", attribute.CallbackMethodName, m_ObjectType));
					m_IsVaild = false;
				}
				else
				{
					if (!string.IsNullOrEmpty(attribute.CanDisplayMethodName))
					{
						m_CanDisplayMethod = m_ObjectType.GetMethod(attribute.CanDisplayMethodName, attribute.CanDisplayMethodBindingFlags);
						if (m_CanDisplayMethod != null)
						{
							if (m_CanDisplayMethod.ReturnType != typeof(bool))
							{
								Debug.LogError(string.Format("return type of the method({0}) is not bool", attribute.CanDisplayMethodName));
								m_CanDisplayMethod = null;
							}
						}
					}

				}
			}
		}

		if (!m_IsVaild)
		{
			return;
		}

		if ((m_CanDisplayMethod == null
				|| (bool)m_CanDisplayMethod.Invoke(property.serializedObject.targetObject, null))
			&& GUI.Button(position, attribute.Text))
		{
			m_CallbackMethod.Invoke(property.serializedObject.targetObject, null);
		}
	}
}