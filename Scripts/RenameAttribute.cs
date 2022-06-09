using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RenameAttribute: PropertyAttribute
{
	public string name;

	public RenameAttribute(string name)
	{
		this.name = name;
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameDraw : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		RenameAttribute rename = (RenameAttribute)attribute;
		EditorGUI.PropertyField(position, property, new GUIContent(rename.name));
		//base.OnGUI(position, property, label);
	}
}
#endif