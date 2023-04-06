using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static AnimationEvents;

public class DeathAnimationEvents : MonoBehaviour
{
    public GameObject heavyClaw;
    Animator anim;
    Control1 c1;
    public bool upwardHeavyClawExists = false;
    public bool horizontalHeavyClawExists = false;
    public bool grabbyClawExists = false;
    [SerializeField] GameObject massiveEyePrefab;
    [SerializeField] GameObject largeEyePrefab;
    [SerializeField] GameObject grabbyClaw;
    public bool eyeExists = false;

    private void Start()
    {
        c1 = GetComponent<Control1>();
        anim = GetComponent<Animator>();
    }

    public void SpawnHeavyClaw()
    {
        if (!upwardHeavyClawExists)
        {
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(1.7f * (c1.facingRight ? 1 : -1), -1, 0),
                Quaternion.Euler(0f, 0f + (c1.facingRight?180:0), 0f));
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            upwardHeavyClawExists = true;
        }
    }

    public void SpawnGrabbyClaw()
    {
        if (!grabbyClawExists)
        {
            GameObject newGrabbyClaw = Instantiate(grabbyClaw, transform.position + new Vector3(0, 1.5f, 0),
                Quaternion.Euler(0f, 0f + (c1.facingRight ? 180 : 0), 0f));
            newGrabbyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            newGrabbyClaw.GetComponent<DeathHeavyClawControl>().grabbyClaw = true;
        }
    }

    public void SpawnHorizontalHeavyClaw()
    {
        if (!horizontalHeavyClawExists)
        {
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(-1.5f * (c1.facingRight ? 1 : -1), 0.1f, 0),
                Quaternion.Euler(0f + (c1.facingRight ? 0 : 180), 0, 90 * (c1.facingRight ? -1 : 1)));
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().horizontal = true;
            horizontalHeavyClawExists = true;

        }
    }

    public void SpawnLargeEye()
    {
        if (!eyeExists)
        {
            GameObject newLargeEye = Instantiate(largeEyePrefab, transform.position, Quaternion.identity);
            newLargeEye.GetComponent<EyeControl>().owner = gameObject;
            eyeExists = true;
        }

    }

    public void SpawnMassiveEye()
    {
        if (!eyeExists)
        {
            GameObject newMassiveEye = Instantiate(massiveEyePrefab, transform.position, Quaternion.identity);
            newMassiveEye.GetComponent<EyeControl>().owner = gameObject;
            eyeExists = true;
        }
    }

    public void IfSpecialHeldContinue()
    {
        if (c1.pim.BufferInputExists(ControlLock.Controls.SPECIAL))
        {
            anim.SetBool("ContinueAttack", true);
        }
    }
}
