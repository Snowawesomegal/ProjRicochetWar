using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.ParticleSystem;
using Unity.Burst.Intrinsics;

public class AnimationEvents : MonoBehaviour
{
    Animator anim;
    ControlLockManager clm;
    ActivateHitbox ah;
    Control1 c1;
    Rigidbody2D rb;
    PlayerInputManager pim;
    ParticleSystem trailps;

    [SerializeField] bool debugMessages;

    [SerializeField] GameObject dirSmokeCloud;
    [SerializeField] GameObject landJumpSmokeCloud;

    [SerializeField] List<Pair<string, int>> animBoolsAndLandingLag;
    [SerializeField] string landingAnimationName;
    [SerializeField] string runAnimationName;
    int landingLag = 0;

    string currentAnimationName = "Idle";
    string lastFrameAnimationName = "Idle";

    void Start()
    {
        anim = GetComponent<Animator>();
        clm = GetComponent<ControlLockManager>();
        ah = GetComponent<ActivateHitbox>();
        c1 = GetComponent<Control1>();
        rb = GetComponent<Rigidbody2D>();
        pim = GetComponent<PlayerInputManager>();
        trailps = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        //ManageCurrentAnimation();

        if (landingLag > 0) // increment landing lag
        {
            landingLag -= 1;
            if (landingLag == 0)
            {
                StopLandingLag();
            }
        }
    }

    void ManageCurrentAnimation() //keeps track of the current animation and animation changes, slightly costly so disabled unless necessary; ran in FixedUpdate()
    {
        currentAnimationName = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (currentAnimationName != lastFrameAnimationName)
        {
            OnAnimationChange();
        }

        lastFrameAnimationName = currentAnimationName;
    }

    void OnAnimationChange() // called on first fixedupdate after the animation node changes; disabled if ManageCurrentAnimation is disabled
    {

    }

    public void CalculateLandingLag() // called by landing animation; see explanations below
    {
        if (!string.IsNullOrEmpty(c1.currentAnimBool)) // if currentAnimBool is not empty, player must have attacked while landing.
        {
            if (debugMessages)
            {
                Debug.Log("Landing lag called StopAnimation; landed while using " + c1.currentAnimBool);
            }

            foreach (Pair<string, int> i in animBoolsAndLandingLag) // Gets landing lag from list of aerials and landing lags
            {
                if (i.left == c1.currentAnimBool)
                {
                    landingLag = i.right; // sets landing lag
                    StartLandingLag(); // freezes animator
                    Debug.Log("LandingLag is now " + landingLag + " frame " + c1.frame);
                }
            }
        }
    }
    void StopLandingLag() // restarts animator and sets landinglag to 0 in case it isn't somehow
    {
        anim.speed = 1;
        landingLag = 0;
    }
    void StartLandingLag() // freezes animator
    {
        anim.speed = 0;
    }

    public void ChangeAnimBool(string boolName, bool toSet, bool changeCurrentAnimBool = true) // Sets entered animBool and updates currentAnimBool appropriately
    //Should always be used when changing animBools in order to keep currentAnimBool up to date.
    {
        if (debugMessages) { Debug.Log("Changed animBool (and currentAnimBool)" + boolName + " to " + toSet); }

        if (boolName.ToLower() != "nothing")
        {
            anim.SetBool(boolName, toSet);
        }

        if (changeCurrentAnimBool)
        {
            if (toSet == true)
            {
                if (Array.Exists(c1.attackBools, i => i == boolName))
                {
                    c1.currentAnimBool = boolName;
                }
            }
            else
            {
                c1.currentAnimBool = null;
            }
        }
    }
    public void UseCurrentAnimBoolToSetAnimBool(int truefalse) // calls anim.SetBool with c1.currentAnimBool. If false, sets currentAnimBool to null.
        // called only by LandingLag atm
    {
        Debug.Log("at the end of landing lag, currentAnimBool is " + c1.currentAnimBool);
        if (truefalse == 0)
        {
            if (!string.IsNullOrEmpty(c1.currentAnimBool))
            {
                anim.SetBool(c1.currentAnimBool, true);
            }
        }
        if (truefalse == 1)
        {
            if (!string.IsNullOrEmpty(c1.currentAnimBool))
            {
                anim.SetBool(c1.currentAnimBool, false);
            }

            c1.currentAnimBool = null;
        }

    }


    /// <summary>
    /// Adds inAnim locker.
    /// </summary>
    public void StartAnimation()
    {
        if (debugMessages)
        {
            Debug.Log("Started animation and added inanim locker on frame " + c1.frame);
        }

        clm.AddLocker(c1.inAnim);
    }

    public void StartDash()
    {
        if (debugMessages)
        {
            Debug.Log("Started dash and added Dashing locker on frame " + c1.frame);
        }

        clm.AddLocker(c1.dashing);
    }

    /// <summary>
    /// Sets current attack's bool to false, continueattack to false, removes inAnim locker, clears connected hitboxes, and stops any lag.
    /// </summary>
    public void StopAnimation(string boolToSetFalse = "Nothing")
    {
        if (debugMessages)
        {
            Debug.Log("stopped animation " + boolToSetFalse + "- frame: " + c1.frame);
        }

        ChangeAnimBool(boolToSetFalse, false, true);

        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.inAnim);
        clm.RemoveLocker(c1.dashing);
        ah.currentConnectedHitboxes.Clear();
        StopLandingLag();
    }
    public void StopAnimationButLeaveCurrentAnimBool(string boolToSetFalse) // same as StopAnimation but does not clear the attack bool so it can be checked to apply landing lag.
        // called only by the start of landing lag atm
    {
        if (debugMessages)
        {
            Debug.Log("stopped animation " + boolToSetFalse + "- frame: " + c1.frame);
        }

        anim.SetBool(boolToSetFalse, false);

        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.inAnim);
        clm.RemoveLocker(c1.dashing);
        ah.currentConnectedHitboxes.Clear();
        StopLandingLag();
    }

    public void SetAnimBoolTrue(string toSetTrue) // Sets a single animation bool true from the animator, and updates currentAnimBool if it is an attack.
    {
        if (debugMessages)
        {
            Debug.Log("set bool " + toSetTrue + " true." + "- frame: " + c1.frame);
        }

        ChangeAnimBool(toSetTrue, true);
    }

    public void SetAnimBoolFalse(string toSetFalse) // Sets a single animation bool false from the animator, and clears currentAnimBool.
    {
        if (debugMessages)
        {
            Debug.Log("set bool " + toSetFalse + " false." + "- frame: " + c1.frame);
        }

        ChangeAnimBool(toSetFalse, false);
    }

    public void StartNewAnimOfMultipart() // Sets ContinueAttack back to false; ContinueAttack is checked from an animation event, and if true, switches to the next part
    {
        anim.SetBool("ContinueAttack", false);
    }

    public void SwitchIfAttacking(string newAnimBool) // Sets a bool to true if attack is inputted; in most cases, this is used with ContinueAttack
    {
        if (pim.BufferInputExists(ControlLock.Controls.ATTACK))
        {
            if (debugMessages)
            {
                Debug.Log("set bool " + newAnimBool + " true. Via SwitchIfNotAttacking." + "- frame: " + c1.frame);
            }

            anim.SetBool(newAnimBool, true);
        }
    }

    public void ApplyHoriForceInAnimation(float force)
    {
        rb.AddForce(new Vector2(force * (c1.facingRight?1:-1), 0));
    }

    public void ApplyVertForceInAnimation(float force)
    {
        rb.AddForce(new Vector2(0, force));
    }

    public void ReduceVelocityByFactor(int factor)
    {
        rb.velocity /= factor;
    }

    public void Dash()
    {
        rb.AddForce(pim.GetCurrentDirectional().current * c1.dashForce);
        StartStopTrail(1);
        clm.RemoveLocker(c1.wallcling);
    }

    public void SpawnDirectionalSmokeCloud()
    {
        float randomDisplacement = UnityEngine.Random.Range(-0.5f, -0.1f) * (c1.facingRight ? 1 : -1);
        GameObject newCloud = Instantiate(dirSmokeCloud, transform.position + new Vector3(-0.5f * (c1.facingRight?1:-1) + randomDisplacement, 0, 0), Quaternion.identity);
        newCloud.GetComponent<SpriteRenderer>().flipX = !c1.facingRight;
        Destroy(newCloud, 0.3f);
    }

    public void SpawnLandJumpSmokeCloud()
    {
        GameObject newCloud = Instantiate(landJumpSmokeCloud, transform.position + new Vector3(-0.5f * (c1.facingRight ? 1 : -1), 0, 0), Quaternion.identity);
        newCloud.GetComponent<SpriteRenderer>().flipX = !c1.facingRight;
        Destroy(newCloud, 0.3f);
    }

    public void StartStopTrail(int startstop)
    {
        if (startstop == 1) { trailps.Play(); }
        else { trailps.Stop(); }
    }

    public void SlowSpeed(float magnitude) // slows speed after a dash finishes. THIS DOES NOT SCALE AUTOMATICALLY; if dash intensity changes a different magnetude must be entered.
    {
        float relevantSpeed = clm.activeLockers.Contains(c1.grounded) ? c1.groundSpeed : c1.airSpeed / 5;

        if (Mathf.Abs(rb.velocity.x) > relevantSpeed)
        {
            rb.AddForce(new Vector2(rb.velocity.x / -magnitude, 0));
        }
        if (Mathf.Abs(rb.velocity.y) > relevantSpeed)
        {
            rb.AddForce(new Vector2(0, rb.velocity.y / -magnitude));
        }
    }

    public void ApplyWallJumpForce() // called by walljump animation
    {
        rb.velocity = Vector2.zero;

        CharacterInput initialJumpInput = pim.GetCachedInput(ControlLock.Controls.JUMP);
        if ((pim.GetCachedInput(ControlLock.Controls.JUMP).Duration >= c1.walljumpShorthopWindow) && initialJumpInput.IsHeld())
        {
            rb.AddForce(1.8f * c1.initialJumpForce * new Vector2(-0.75f * c1.collidedWallSide, c1.wallJumpVerticalOoOne));
        }
        else
        {
            rb.AddForce(1.4f * c1.initialJumpForce * new Vector2(-0.75f * c1.collidedWallSide, c1.wallJumpVerticalOoOne));
        }
        clm.RemoveLocker(c1.wallcling);
        c1.collidedWallSide = 0;
        c1.touchingWall = false;
        c1.wallTouching = null;

        // it is ridiculous how many things I have to set here, something about wall mechanics should probably be reworked
    }

    public void ApplyJumpForce() // called by jump animation
    {
        CharacterInput initialJumpInput = pim.GetCachedInput(ControlLock.Controls.JUMP);

        if ((initialJumpInput.Duration >= c1.shorthopWindow) && initialJumpInput.IsHeld())
        {
            rb.AddForce(1.5f * c1.initialJumpForce * Vector2.up);
        }
        else
        {
            rb.AddForce(c1.initialJumpForce * Vector2.up);
        }
    }

    public void PlaySoundFromAnimator(string name)
    {
        c1.am.PlaySound(name);
    }

    public void PlaySoundGroupFromAnimator(string name)
    {
        c1.am.PlaySoundGroup(name);
    }
}
