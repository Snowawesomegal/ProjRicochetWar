using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterSelectorButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] FighterSelection fighter;

    void Awake()
    {
        image.sprite = fighter.showcaseSprite;
    }
}
