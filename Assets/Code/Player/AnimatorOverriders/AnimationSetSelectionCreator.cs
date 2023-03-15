using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AnimationSetSelectionCreator : MonoBehaviour
{
    [SerializeField] public Text animationSetName;
    [SerializeField] public Image animationSetImage;
    [SerializeField] public int animationSetIndex;

    public PlayerAnimationSet animationSet;

    public PlayerAnimationSetter playerAnimationSetter;

    public void Start()
    {
        if (!animationSetName)
            animationSetName = GetComponentInChildren<Text>();
        if (!animationSetImage)
            animationSetImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerAnimationSet animationSet, PlayerAnimationSetter animationSetter, int setIndex)
    {
        this.animationSet = animationSet;
        this.playerAnimationSetter = animationSetter;

        this.animationSetImage.sprite = animationSet.showcaseImage;
        this.animationSetName.text = animationSet.setName;
        this.animationSetIndex = setIndex;
    }

    public void SelectSkin()
    {
        if (playerAnimationSetter)
            playerAnimationSetter.Set(animationSet.animationSet, animationSetIndex);
    }
}
