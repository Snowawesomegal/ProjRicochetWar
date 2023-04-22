using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(Pair<,>), true)]
public class PairPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var leftRect = new Rect(position.x, position.y, position.width / 2 - 0.5f, position.height);
        var rightRect = new Rect(position.x + position.width / 2 + 0.5f, position.y, position.width / 2 - 0.5f, position.height);

        EditorGUI.PropertyField(leftRect, property.FindPropertyRelative("left"), GUIContent.none);
        EditorGUI.PropertyField(rightRect, property.FindPropertyRelative("right"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}