using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VFX/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [SerializeField] public new string name;
    [SerializeField] public List<Color> colors;
}
