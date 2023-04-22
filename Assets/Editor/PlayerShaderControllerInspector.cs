using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(PlayerShaderController))]
public class PlayerShaderControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerShaderController myTarget = (PlayerShaderController)target;

        if (EditorSceneManager.IsPreviewScene(myTarget.gameObject.scene))
        {
            EditorGUILayout.HelpBox("Unable to initialize shader because you are in preview mode.", MessageType.Info);
            return;
        }

        if (EditorUtility.IsPersistent(myTarget.gameObject)) // returns false if object lives in scene
        {
            EditorGUILayout.HelpBox("Unable to initialize shader because it is not instantiated (in the scene) yet.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Reset Material"))
        {
            myTarget.ResetMaterial();
        }

        EditorGUILayout.Space(10);

        myTarget.ShaderColor = EditorGUILayout.ColorField("Shader Flash Color", myTarget.ShaderColor);
        myTarget.ShaderStrength = EditorGUILayout.Slider("Shader Flash Strength", myTarget.ShaderStrength, 0, 1);

        EditorGUILayout.Space(10);

        myTarget.palette_selector_dropdown = EditorGUILayout.Foldout(myTarget.palette_selector_dropdown, "Palette Selector");
        if (myTarget.palette_selector_dropdown)
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
            if (myTarget.samplePalette != null && GUILayout.Button("Refresh Sample Palette: " + myTarget.samplePalette.name))
            {
                myTarget.RefreshSamplePalette();
            }
            if (myTarget.samplePalette != null && GUILayout.Button("Reset Target Palette to Sample"))
            {
                myTarget.SetTargetPalette(myTarget.samplePalette);
            }
            foreach (ColorPalette palette in myTarget.targetPalettes)
            {
                if (palette != null && GUILayout.Button("Set Target Palette: " + palette.name))
                {
                    myTarget.SetTargetPalette(palette);
                }
            }
            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }

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

                EditorGUILayout.Space(5);
            }
            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }
    }
}
