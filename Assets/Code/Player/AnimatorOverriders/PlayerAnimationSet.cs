using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animation Set", menuName = "Characters/Animation Set")]
public class PlayerAnimationSet : ScriptableObject
{
    public AnimatorOverrideController animationSet;

    public Sprite showcaseImage;

    public string setName;
}
