using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Animation Collection", menuName = "Characters/Animation Collection")]
public class AnimationSetCollection : ScriptableObject
{
    [SerializeField] public List<PlayerAnimationSet> animationSets;
}
