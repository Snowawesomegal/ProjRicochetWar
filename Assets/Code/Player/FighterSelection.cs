using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fighter Selection")]
public class FighterSelection : ScriptableObject
{
    [SerializeField] public string fighterName;
    [SerializeField] public Control1 fighter;
    [SerializeField] public Sprite showcaseSprite;
}
