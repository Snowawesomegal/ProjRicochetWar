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
    Rigidbody2D rb;
    public bool upwardHeavyClawExists = false;
    public bool horizontalHeavyClawExists = false;
    public bool grabbyClawExists = false;
    [SerializeField] GameObject massiveEyePrefab;
    [SerializeField] GameObject largeEyePrefab;
    [SerializeField] GameObject grabbyClaw;
    public bool eyeExists = false;
    public float movementAbilityAccel = 3;
    public int movementAbilityMinFrames = 20;
    public int movementAbilityCurrentFrames = 0;
    public int movementAbilityMaxFrames = 120;


    private void Start()
    {
        c1 = GetComponent<Control1>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        if (c1.clm.activeLockers.Contains(c1.UNIQUEinMovementAir) || c1.clm.activeLockers.Contains(c1.UNIQUEinMovementGround))
        {
            rb.AddForce(c1.pim.GetCurrentDirectional().current * movementAbilityAccel);
            if (rb.velocity.y != 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(Mathf.MoveTowards(rb.velocity.y, 0, 1), -c1.airSpeed, c1.airSpeed));
            }

            movementAbilityCurrentFrames += 1;
            if (movementAbilityCurrentFrames > movementAbilityMinFrames)
            {
                if (!c1.pim.GetCachedInput(ControlLock.Controls.MOVEMENT).IsHeld())
                {
                    TransitionToEndSpecial();
                }
                else if (movementAbilityCurrentFrames > movementAbilityMaxFrames)
                {
                    TransitionToEndSpecial();
                }
            }
        }
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
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(-0.8f * (c1.facingRight ? 1 : -1), 0.1f, 0),
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
            GameObject newLargeEye = Instantiate(largeEyePrefab, transform.position + new Vector3(0.9f * (c1.facingRight ? 1 : -1), 0.2f, 0), Quaternion.identity);
            newLargeEye.GetComponent<EyeControl>().owner = gameObject;
            eyeExists = true;
        }

    }

    public void OnSpecialStartup()
    {
        movementAbilityCurrentFrames = 0;

        if (c1.grounded)
        {
            c1.clm.AddLocker(c1.inAnim);
        }
        else
        {
            c1.clm.AddLocker(c1.inAerialAnim);
        }

        Debug.Log("on special startup");
        rb.velocity = Vector2.zero;
        c1.canFastFall = false;
        c1.affectedByGravity = false;
    }

    public void OnEnterSpecial()
    {
        c1.clm.RemoveLocker(c1.inAnim);
        c1.clm.RemoveLocker(c1.inAerialAnim);
        if (c1.grounded)
        {
            c1.clm.AddLocker(c1.UNIQUEinMovementGround);
        }
        else
        {
            c1.clm.AddLocker(c1.UNIQUEinMovementAir);
        }

        c1.ChangeIntangible(true);
        c1.permaTrailps.Stop();
    }

    void TransitionToEndSpecial()
    {
        clmEx.RemoveAllLockersExcept(c1.clm, new StandardControlLocker[] { c1.grounded, c1.airborne });
        if (c1.grounded)
        {
            c1.clm.AddLocker(c1.inAnim);
        }
        else
        {
            c1.clm.AddLocker(c1.inAerialAnim);
        }

        c1.ae.ChangeAnimBool("ContinueAttack", true);

        movementAbilityCurrentFrames = 0;
        c1.ChangeIntangible(false);
        c1.affectedByGravity = true;
        c1.canFastFall = true;
        c1.permaTrailps.Play();
    }

    public void SpawnMassiveEye()
    {
        if (!eyeExists)
        {
            GameObject newMassiveEye = Instantiate(massiveEyePrefab, transform.position + new Vector3(0.9f * (c1.facingRight ? 1 : -1), 0.2f, 0), Quaternion.identity);
            newMassiveEye.GetComponent<EyeControl>().owner = gameObject;
            eyeExists = true;
        }
    }

    public void IfSpecialHeldContinue()
    {
        if (c1.pim.BufferInputExists(ControlLock.Controls.SPECIAL))
        {
            c1.ae.ChangeAnimBool("ContinueAttack", true);
        }
    }
}
