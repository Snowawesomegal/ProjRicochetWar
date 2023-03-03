using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerShaderController))]
public class PlayerShaderControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerShaderController myTarget = (PlayerShaderController)target;

        EditorGUILayout.Space(10);

        myTarget.ShaderColor = EditorGUILayout.ColorField(myTarget.ShaderColor);

        EditorGUILayout.Space(10);

        myTarget.ShaderStrength = EditorGUILayout.Slider(myTarget.ShaderStrength, 0, 1);
    }
}
