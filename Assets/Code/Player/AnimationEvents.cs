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

    public void DebugInfo()
    {

    }

    public void StartAnimation()
    {
        clm.AddLocker(c1.inAnim);
    }

    public void StopAnimation(string boolToSetFalse)
    {
        anim.SetBool(boolToSetFalse, false);
        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.inAnim);
        ah.currentConnectedHitboxes.Clear();
    }

    public void StartNewAnimOfMultihit()
    {
        anim.SetBool("ContinueAttack", false);
    }

    public void SwitchIfAttacking(string newAnimBool)
    {
        if (pim.BufferInputExists(ControlLock.Controls.ATTACK))
        {
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

}
