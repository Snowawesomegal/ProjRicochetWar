using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterSelectorButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] public FighterSelection fighter;
    [SerializeField] public RectTransform rect;

    void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        image.sprite = fighter.showcaseSprite;
    }
}
