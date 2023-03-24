using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageRunAudio : StateMachineBehaviour
{
    bool active = true;
    bool soundPlaying = false;
    [SerializeField] string sound;
    AudioManager am;

    private void Awake()
    {
        am = GameObject.Find("SettingsManager").GetComponent<AudioManager>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        active = true;
        soundPlaying = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (active)
        {
            if (!soundPlaying)
            {
                if (animator.GetFloat("HorizontalInput") != 0)
                {
                    am.PlaySound(sound);
                    soundPlaying = true;
                }
            }
            else
            {
                if (animator.GetFloat("HorizontalInput") == 0)
                {
                    EndSound();
                }
            }
        }    
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EndSound();
        active = false;
    }

    void EndSound()
    {
        am.StopSound(sound);
        soundPlaying = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
