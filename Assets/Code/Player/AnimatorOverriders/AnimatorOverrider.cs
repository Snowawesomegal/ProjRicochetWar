using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorOverrider : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (!_animator)
            _animator = GetComponent<Animator>();
    }

    public void SetAnimations(AnimatorOverrideController overrideController, int skinIndex)
    {
        _animator.runtimeAnimatorController = overrideController;
        //if (_animator.GetInteger("EquipSkin") != skinIndex)
        //    _animator.SetInteger("EquipSkin", skinIndex);
    }
}
