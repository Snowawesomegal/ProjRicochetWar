using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShaderController : MonoBehaviour
{
    public const int NUMBER_COLORS = 10;

    [SerializeField] Renderer renderer;
    [HideInInspector] Material material;

    [HideInInspector] public bool color_selector_dropdown;
    [HideInInspector] public bool palette_selector_dropdown;

    [SerializeField] public ColorPalette samplePalette;
    [SerializeField] public List<ColorPalette> targetPalettes = new List<ColorPalette>();

    private void Awake()
    {
        ValidateMaterial();
    }

    // Ensure that the material is present
    public bool ValidateMaterial()
    {
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
            if (renderer == null)
                return false;
        }
        if (material == null)
        {
            if (renderer == null)
                return false;
            ResetMaterial();
        }
        return true;
    }

    public void ResetMaterial()
    {
        // Reset the material being used
        bool removeTossedMaterial = (material != null);
        material = renderer.material;
        // This is called in order to clean up random materials that generated and lost their last reference.
        // Without this, we get a material leak in the editor: https://answers.unity.com/questions/283271/material-leak-in-editor.html
        if (removeTossedMaterial)
            Resources.UnloadUnusedAssets();
    }

    public float ShaderStrength { 
        get { return ValidateMaterial() ? material.GetFloat("_Hurt") : 0f; } 
        set { if (!ValidateMaterial()) return; material.SetFloat("_Hurt", value); } 
    }
    public Color ShaderColor { 
        get { return ValidateMaterial() ? material.GetColor("_Color") : Color.black; } 
        set { if (!ValidateMaterial()) return; material.SetColor("_Color", value); } 
    }
    public Material ShaderMaterial
    {
        get { return ValidateMaterial() ? material : null; }
    }

    public void RefreshSamplePalette()
    {
        if (samplePalette == null)
        {
            Debug.LogWarning("Trying to refresh sample palette... palette is null.");
            return;
        }
        if (!ValidateMaterial())
            return;

        for (int i = 0; i < NUMBER_COLORS; i++)
        {
            int currentName = i + 1;
            string sampleName = "_ColorSample" + currentName;
            if (i < samplePalette.colors.Count)
            {
                material.SetColor(sampleName, samplePalette.colors[i]);
            }
            else
            {
                material.SetColor(sampleName, Color.black);
            }
        }
    }

    public void SetTargetPalette(ColorPalette palette)
    {
        if (!ValidateMaterial())
        {
            Debug.LogWarning("Trying to set palette " + palette.name + " but material is invalid.");
            return;
        }

        for (int i = 0; i < NUMBER_COLORS; i++)
        {
            int currentName = i + 1;
            string sampleName = "_SampleColor" + currentName;
            string targetName = "_TargetColor" + currentName;
            if (i < palette.colors.Count)
            {
                material.SetColor(targetName, palette.colors[i]);
            }
            else
            {
                // if no color exists, set the target to the current sample
                // this will avoid changing the color
                material.SetColor(targetName, material.GetColor(sampleName));
            }
        }
    }
}
