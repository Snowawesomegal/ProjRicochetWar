using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShaderController : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    [HideInInspector] Material material;

    private void Awake()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();
        if (material == null)
            material = renderer.material;
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
}
