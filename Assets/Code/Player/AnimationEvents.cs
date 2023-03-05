using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.ParticleSystem;

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

    [SerializeField] GameObject smokeCloud;

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
    /// Sets bool to false, continueattack to false, removes inAnim locker, and clears connected hitboxes.
    /// </summary>
    public void StopAnimation(string boolToSetFalse = "Nothing")
    {
        if (debugMessages)
        {
            Debug.Log("stopped animation " + boolToSetFalse + "- frame: " + c1.frame);
        }

        if (boolToSetFalse.ToLower() != "nothing")
        {
            anim.SetBool(boolToSetFalse, false);
        }

        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.inAnim);
        clm.RemoveLocker(c1.dashing);
        ah.currentConnectedHitboxes.Clear();
    }

    public void SetAnimBoolTrue(string toSetTrue)
    {
        if (debugMessages)
        {
            Debug.Log("set bool " + toSetTrue + " true." + "- frame: " + c1.frame);
        }

        c1.ChangeAnimBool(toSetTrue, true);
    }

    public void SetAnimBoolFalse(string toSetFalse)
    {
        if (debugMessages)
        {
            Debug.Log("set bool " + toSetFalse + " false." + "- frame: " + c1.frame);
        }

        c1.ChangeAnimBool(toSetFalse, false);
    }

    public void StartNewAnimOfMultihit()
    {
        anim.SetBool("ContinueAttack", false);
    }

    public void SwitchIfAttacking(string newAnimBool)
    {
        if (pim.BufferInputExists(ControlLock.Controls.ATTACK))
        {
            if (debugMessages)
            {
                Debug.Log("set bool " + newAnimBool + " true. Via SwitchIfNotAttacking" + " true." + "- frame: " + c1.frame);
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

    public void SpawnSmokeCloud()
    {
        GameObject newCloud = Instantiate(smokeCloud, transform.position + new Vector3(-0.5f * (c1.facingRight?1:-1), 0, 0), Quaternion.identity);
        newCloud.GetComponent<SpriteRenderer>().flipX = !c1.facingRight;
        Destroy(newCloud, 0.3f);
    }

    public void StartStopTrail(int startstop)
    {
        if (startstop == 1) { trailps.Play(); }
        else { trailps.Stop(); }
    }

    public void SlowSpeed(float magnitude)
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
