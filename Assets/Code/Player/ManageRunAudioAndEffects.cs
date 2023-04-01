using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageRunAudioAndEffects : StateMachineBehaviour
{
    bool stateActive = true;
    bool currentlyMoving = false;
    [SerializeField] string sound;
    AudioManager am;
    EffectManager em;
    [SerializeField] List<Pair<string, Vector2>> toSpawnOnRunStart;
    bool soundExists;

    private void Awake()
    {
        am = GameObject.Find("SettingsManager").GetComponent<AudioManager>();
        em = am.GetComponent<EffectManager>();

        soundExists = !string.IsNullOrEmpty(sound);
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateActive = true;
        currentlyMoving = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateActive)
        {
            float horiInput = animator.GetFloat("HorizontalInput");
            if (!currentlyMoving)
            {
                if (horiInput != 0)
                {
                    currentlyMoving = true;

                    if (soundExists)
                    {
                        am.PlaySound(sound);
                    }

                    foreach (Pair<string, Vector2> i in toSpawnOnRunStart)
                    {
                        em.SpawnDirectionalEffect(i.left, animator.transform.position + new Vector3(i.right.x * (horiInput > 0 ? 1 : -1), i.right.y, 0), horiInput > 0);
                    }
                }
            }
            else
            {
                if (horiInput == 0)
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
        stateActive = false;
    }

    void EndSound()
    {
        if (soundExists)
        {
            am.StopSound(sound);
        }

        currentlyMoving = false;
    }
}
