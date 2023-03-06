using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationSetManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] public PlayerAnimationSet blankCollection;
    [SerializeField] public AnimationSetCollection animationCollection;

    // Start is called before the first frame update
    void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    public void ResetAnimations()
    {
        animator.runtimeAnimatorController = blankCollection.animationSet;
    }

    public void SetAnimations(AnimatorOverrideController overrideController)
    {
        animator.runtimeAnimatorController = overrideController;
    }
    public void SetAnimations(AnimatorOverrideController overrideController, int skinIndex)
    {
        animator.runtimeAnimatorController = overrideController;
    }

    public void SetCurrentAnimationByIndex(int index)
    {
        if (!animationCollection)
        {
            Debug.LogWarning("Animation Collection is null... cannot set current animation set by index.");
        }
        if (index >= 0 && index < animationCollection.animationSets.Count)
        {
            SetAnimations(animationCollection.animationSets[index].animationSet, index);
        } else
        {
            Debug.LogWarning("IndexOutOfBounds: Index to set current animation set is not within bounds.");
        }
    }
}
