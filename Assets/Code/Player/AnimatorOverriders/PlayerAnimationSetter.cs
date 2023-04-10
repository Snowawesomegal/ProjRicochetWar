using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSetter : MonoBehaviour
{
    [SerializeField] public AnimationSetCollection animationSetCollection;
    [SerializeField] private AnimatorOverrider overrider;
    [SerializeField] private int defaultSkinIndex;
    [SerializeField] Animator animator;

    private void Awake()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        // Set(defaultSkinIndex);
        AnimationSetSelector animationSetSelector = FindObjectOfType<AnimationSetSelector>();
        if (animationSetSelector)
        {
            animationSetSelector.SetPlayer(this);
            Debug.Log("Set PlayerSpriteSetter");
        } else
        {
            Debug.Log("Did not find SkinSelector");
        }
    }

    public void Set(int i)
    {
        if (i >= animationSetCollection.animationSets.Count)
            i = 0;
        if (animationSetCollection.animationSets.Count <= 0) {
            Debug.LogError("Override Controllers array is empty.");
            return;
        }
        overrider.SetAnimations(animationSetCollection.animationSets[i].animationSet, i);
    }

    public void Set(AnimatorOverrideController animatorOverrideController, int skinIndex)
    {
        if (animatorOverrideController != null)
        {
            overrider.SetAnimations(animatorOverrideController, skinIndex);
            if (animator.GetInteger("EquipSkin") != skinIndex)
                animator.SetInteger("EquipSkin", skinIndex);
        } else
        {
            Debug.LogError("Error -trying to set null skin");
        }
    }
}
