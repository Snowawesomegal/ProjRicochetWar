using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShaderController : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    [SerializeField] Material material;

    private void Awake()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();
        if (material == null)
            material = renderer.material;
    }

    public void SetupMaterial()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();
        if (material == null)
            material = renderer.material;
    }

    public float ShaderStrength { 
        get { if (material == null) SetupMaterial(); return material.GetFloat("_Hurt"); } 
        set { if (material == null) SetupMaterial(); material.SetFloat("_Hurt", value); } 
    }
    public Color ShaderColor { 
        get { if (material == null) SetupMaterial(); return material.GetColor("_Color"); } 
        set { if (material == null) SetupMaterial(); material.SetColor("_Color", value); } 
    }
}
