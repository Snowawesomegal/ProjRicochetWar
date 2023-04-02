using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.ParticleSystem;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using System.Xml.Linq;

public class AnimationEvents : MonoBehaviour
{
    Animator anim;
    ControlLockManager clm;
    ActivateHitbox ah;
    Control1 c1;
    Rigidbody2D rb;
    PlayerInputManager pim;
    GameObject sm;
    EffectManager em;
    HitboxInteractionManager him;

    [SerializeField] bool debugMessages;

    [SerializeField] List<Pair<string, int>> animBoolsAndLandingLag;
    List<string> attackBools = new List<string>();
    [SerializeField] string landingAnimationName;
    [SerializeField] string runAnimationName;

    [SerializeField] List<ObjectToInstantiate> projectiles;

    void Start()
    {
        anim = GetComponent<Animator>();
        clm = GetComponent<ControlLockManager>();
        ah = GetComponent<ActivateHitbox>();
        c1 = GetComponent<Control1>();
        rb = GetComponent<Rigidbody2D>();
        pim = GetComponent<PlayerInputManager>();
        sm = GameObject.Find("SettingsManager");
        em = sm.GetComponent<EffectManager>();
        him = Camera.main.GetComponent<HitboxInteractionManager>();

        foreach (Pair<string, int> i in animBoolsAndLandingLag)
        {
            attackBools.Add(i.left);
        }
    }

    public void CalculateLandingLag() // called by landing animation; see explanations below
    {
        if (!string.IsNullOrEmpty(c1.currentAnimBool)) // if currentAnimBool is not empty, player must have attacked while landing.
        {
            foreach (Pair<string, int> i in animBoolsAndLandingLag) // Gets landing lag from list of aerials and landing lags
            {
                if (i.left == c1.currentAnimBool)
                {
                    StartLandingLag(i.right); // freezes animator using the listed number of frames
                }
            }
        }
    }

    void StartLandingLag(int frameLength) // freezes animator
    {
        c1.FreezeFrames(0, frameLength);
    }

    void StopLandingLag()
    {
        if (GameManager.Instance.TimeController.GetTimeScale(c1) != 1)
        {
            GameManager.Instance.TimeController.RemoveSlows(c1);
        }
    }

    public void ChangeAnimBool(string boolName, bool toSet, bool changeCurrentAnimBool = true) // Sets entered animBool and updates currentAnimBool appropriately
    //Should always be used when changing animBools in order to keep currentAnimBool up to date.
    {
        if (!string.IsNullOrEmpty(boolName))
        {
            if (boolName.ToLower() != "nothing")
            {
                anim.SetBool(boolName, toSet);
            }
        }

        if (changeCurrentAnimBool)
        {
            if (debugMessages) { Debug.Log("Changed animBool (and currentAnimBool) " + boolName + " to " + toSet + " -- frame " + c1.frame); }
            if (toSet == true)
            {
                if (debugMessages) { Debug.Log(attackBools.Contains(boolName) + ", attackBools contains " + boolName); }
                if (attackBools.Contains(boolName))
                {
                    c1.currentAnimBool = boolName;
                }
            }
            else
            {
                c1.currentAnimBool = null;
            }
        }
        else
        {
            if (debugMessages) { Debug.Log("Changed animBool, but NOT currentAnimbool to " + boolName + " to " + toSet + " -- frame " + c1.frame); }
        }
    }

    public void UseCurrentAnimBoolToSetAnimBool(int truefalse) // calls anim.SetBool with c1.currentAnimBool. If false, sets currentAnimBool to null.
        // called only by LandingLag atm
    {
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

    public void StartAnimation()
    {
        if (debugMessages)
        {
            Debug.Log("Started animation and added inanim locker on frame " + c1.frame);
        }

        if (clm.activeLockers.Contains(c1.grounded))
        {
            clm.AddLocker(c1.inAnim);
        }
        else
        {
            clm.AddLocker(c1.inAerialAnim);
        }

        foreach (GameObject i in him.doNotEnableHitboxes)
        {
            i.GetComponent<HitboxInfo>().doNotEnable = false;
        }
        him.doNotEnableHitboxes.Clear();
    }

    public void StartSpecial()
    {
        if (debugMessages)
        {
            Debug.Log("Started special and added inanim locker on frame " + c1.frame);
        }

        clm.AddLocker(c1.inAnim);

        foreach (GameObject i in him.doNotEnableHitboxes)
        {
            i.GetComponent<HitboxInfo>().doNotEnable = false;
        }
        him.doNotEnableHitboxes.Clear();
    }

    public void StartDash()
    {
        if (debugMessages)
        {
            Debug.Log("Started dash and added Dashing locker on frame " + c1.frame);
        }

        clm.AddLocker(c1.dashing);
        c1.affectedByGravity = false;
        c1.intangible = true;
        c1.ignoreFriction = true;
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

        if (!string.IsNullOrEmpty(boolToSetFalse))
        {
            ChangeAnimBool(boolToSetFalse, false, true);
        }

        c1.affectedByGravity = true;
        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.hitstun);
        clm.RemoveLocker(c1.inAnim);
        clm.RemoveLocker(c1.dashing);
        clm.RemoveLocker(c1.inGrab);
        clm.RemoveLocker(c1.inAerialAnim);
        c1.affectedByGravity = true;
        c1.intangible = false;
        c1.ignoreFriction = false;
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
        c1.affectedByGravity = true;
        anim.SetBool("ContinueAttack", false);
        clm.RemoveLocker(c1.inAnim);
        clm.RemoveLocker(c1.dashing);
        clm.RemoveLocker(c1.inAerialAnim);
        c1.affectedByGravity = true;
        c1.intangible = false;
        c1.ignoreFriction = false;
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

    public void IgnoreFrictionOnOff(int onoff)
    {
        if (onoff == 1)
        {
            c1.ignoreFriction = true;
        }
        else
        {
            c1.ignoreFriction = false;
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
        Vector2 currentDir = pim.GetCurrentDirectional().current;
        if (currentDir != Vector2.zero)
        {
            rb.AddForce(currentDir * c1.dashForce);
        }
        else
        {
            rb.AddForce(new Vector2(1 * (c1.facingRight ? 1 : -1), 0) * c1.dashForce);
        }

        StartStopTrail(1);
        clm.RemoveLocker(c1.wallcling);
    }

    public void SpawnGhostCounterShield()
    {
        c1.affectedByGravity = false;
        c1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        Vector2 currentDir = c1.pim.GetCurrentDirectional().current;
        float rotationz;
        if (currentDir != new Vector2(0, 0))
        {
            rotationz = Vector2.Angle(new Vector2(1, 0), c1.pim.GetCurrentDirectional().current);
            if (c1.pim.GetCurrentDirectional().current.y < 0)
            {
                rotationz *= -1;
            }
        }
        else
        {
            rotationz = c1.facingRight ? 0 : -180;
        }
        Vector2 offset = Vector2.zero;
        if (currentDir == Vector2.zero)
        {
            offset.x = c1.facingRight ? 1 : -1;
        }
        else
        {
            offset = currentDir;
        }

        Instantiate(c1.counterShield, transform.position + (Vector3) offset, Quaternion.Euler(0, 0, rotationz)).GetComponent<SimpleAnimationEvents>().owner = gameObject;

    }

    public void SpawnDirectionalSmokeCloud()
    {
        em.SpawnDirectionalEffect("RunSmokeCloud", transform.position, c1.facingRight, UnityEngine.Random.Range(-0.5f, -0.1f) * (c1.facingRight ? 1 : -1));
    }

    public void SpawnBasicDirEffect(string name)
    {
        em.SpawnDirectionalEffect(name, transform.position, c1.facingRight);
    }

    public void SpawnEffectAtFeet(string name)
    {
        em.SpawnDirectionalEffect(name, transform.position + new Vector3 (c1.feetOffset.x * (c1.facingRight ? 1 : -1), c1.feetOffset.y, 0), c1.facingRight);
    }

    public void StartStopTrail(int startstop)
    {
        if (c1.dashps != null)
        {
            if (startstop == 1)
            {
                c1.dashps.Play();
            }
            else
            {
                c1.dashps.Stop();
            }
        }
    }

    public void SlowSpeed(float magnitude) // slows speed after a dash finishes; called by animation event. THIS DOES NOT SCALE AUTOMATICALLY; if dash intensity changes a different magnetude must be entered.
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

    public void InstantiateObjectFromAnimation(string name)
    {
        foreach (ObjectToInstantiate i in projectiles)
        {
            if (i.name == name)
            {
                GameObject newProj = Instantiate(i.prefab, transform.position + new Vector3(i.offset.x * (c1.facingRight ? 1 : -1), i.offset.y, 0),
                    Quaternion.Euler(new Vector3(0, 0, i.angle * (c1.facingRight ? 1 : -1))));
                Destroy(newProj, i.lifetime * 0.01666666666f);

                if (newProj.TryGetComponent(out SimpleAnimationEvents sae))
                {
                    sae.owner = gameObject;
                }

                return;
            }
        }
        Debug.LogWarning("Tried to spawn " + name + " but " + gameObject.name + " does not have an object with that name in its projectile list.");
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

    [Serializable]
    public class ObjectToInstantiate
    {
        public GameObject prefab;
        public string name;
        public float angle;
        public Vector2 offset;
        public int lifetime;
    }
}
