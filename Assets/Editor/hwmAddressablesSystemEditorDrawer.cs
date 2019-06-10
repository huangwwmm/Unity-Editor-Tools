using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(hwmAddressablesSystemEditorAttribute))]
public sealed class hwmAddressablesSystemEditorDrawer : PropertyDrawer
{
	private const float PROPERTY_SPACING_HEIGHT = 3.6f;
	private const int PROPERTY_COUNT = 3;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		hwmAddressablesSystemConfig config = property.serializedObject.targetObject as hwmAddressablesSystemConfig;
		float propertyHeight = base.GetPropertyHeight(property, label);
		position.height = propertyHeight;
		if (config == null)
		{
			EditorGUI.LabelField(position, "AddressablesSystemEditorAttribute can only be used in AddressablesSystemConfig ");
		}
		else
		{
			if (GUI.Button(position, "Generate All"))
			{
				hwmAddressablesSystemUtility.GenerateAll(config);
			}

			position.y += propertyHeight + PROPERTY_SPACING_HEIGHT;
			if (GUI.Button(position, "Check Config"))
			{
				hwmAddressablesSystemUtility.CheckConfig(config);
			}

			position.y += propertyHeight + PROPERTY_SPACING_HEIGHT;
			if (GUI.Button(position, "Help"))
			{
				EditorUtility.DisplayDialog("Addressables System Config", "这么简单的东西还需要帮助？", "OK");
			}
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) * PROPERTY_COUNT + PROPERTY_SPACING_HEIGHT * (PROPERTY_COUNT - 1);
	}
}