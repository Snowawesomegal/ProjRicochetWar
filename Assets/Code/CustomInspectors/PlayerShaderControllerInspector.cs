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

        if (GUILayout.Button("Reset Material"))
        {
            myTarget.ResetMaterial();
        }

        EditorGUILayout.Space(10);

        myTarget.ShaderColor = EditorGUILayout.ColorField("Shader Flash Color", myTarget.ShaderColor);

        EditorGUILayout.Space(10);

        myTarget.ShaderStrength = EditorGUILayout.Slider("Shader Flash Strength", myTarget.ShaderStrength, 0, 1);

        EditorGUILayout.Space(10);

        myTarget.color_selector_dropdown = EditorGUILayout.Foldout(myTarget.color_selector_dropdown, "Color Replacements");
        if (myTarget.color_selector_dropdown)
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
            for (int i = 1; i <= 10; i++)
            {
                string sampleColor = "_SampleColor" + i;
                string targetColor = "_TargetColor" + i;

                Color sampleColorValue = EditorGUILayout.ColorField("Sample Color " + i, myTarget.ShaderMaterial.GetColor(sampleColor));
                myTarget.ShaderMaterial.SetColor(sampleColor, sampleColorValue);

                Color targetColorValue = EditorGUILayout.ColorField("Target Color " + i, myTarget.ShaderMaterial.GetColor(targetColor));
                myTarget.ShaderMaterial.SetColor(targetColor, targetColorValue);

                EditorGUILayout.Space(10);
            }
            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }
    }
}
