using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerAnimationSetManager))]
public class PlayerAnimationSetManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerAnimationSetManager myTarget = (PlayerAnimationSetManager)target;

        if (GUILayout.Button("Reset Animations"))
        {
            myTarget.ResetAnimations();
        }

        foreach (PlayerAnimationSet animSet in myTarget.animationCollection.animationSets)
        {
            if (GUILayout.Button("Set Animations: " + animSet.setName))
            {
                myTarget.SetAnimations(animSet.animationSet);
            }
        }
    }
}
