using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System.Drawing;
using System.Linq;
using UnityEngine.TextCore.Text;
using System.Runtime;
using Unity.VisualScripting;
using System.Security.Cryptography;

public class Control1 : MonoBehaviour, IIdentifiable
{
    // Todo list
    // add Death's heavy aerials

    private uint id;
    private bool initializedID;
    bool IIdentifiable.InitializedID { get { return initializedID; } set { initializedID = value; } }
    uint IIdentifiable.ID { get { return id; } set { id = value; } }

    public GameObject testCircle;
    public Vector3 feetOffset;
    float minimumHitstunFrames = 0;

    List<string> animatorBools = new List<string>();

    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;
    [SerializeField] float airFriction = 500;
    public float wallJumpVerticalOoOne = 0.5f;
    public bool intangible = false;
    public bool canDoubleJump = true;

    [SerializeField] float DIStrength = 5;
    int knockbackTime = 0;
    Vector2 initialLaunchVelocity;
    Pair<HitboxInfo, Vector2> queuedKnockback;
    GameObject runeActive;
    [SerializeField] bool DLightBounce = true;
    public bool firstFrameOfHitstun = false;

    // "Do not" booleans
    public bool doNotUpdatePlatformCollider = false;
    public bool canFastFall = true;
    public bool ignoreFriction = false;
    public bool affectedByGravity = true;

    //speeds
    public float airSpeed = 10;
    [SerializeField] float airAccel = 50;
    [SerializeField] float airAccelDuringAerial = 40;
    public float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    public float initialJumpForce = 10;
    public float dashForce = 10;
    [SerializeField] float overMaxYSpeedAdjustment = 10;
    [SerializeField] float overMaxXSpeedAdjustment = 10;
    [SerializeField] float fastFallMultiplier = 1.5f;
    [SerializeField] float doubleJumpMultiplier = 1.4f;
    public bool applyFFMultiplier = false;
    public int delayFF = 0;
    Vector2 beforeFreezeSpeed = Vector2.zero;
    float moveDiMultiplier = 1;
    [SerializeField] float chargeRecoverySpeed = 5;

    //Costs
    [SerializeField] float dashCost = 50;

    //windows
    public float shorthopWindow = 3;
    public float walljumpShorthopWindow = 3;

    //components
    Rigidbody2D rb;
    Collider2D bc;
    Animator anim;
    SpriteRenderer sr;
    public ControlLockManager clm;
    public PlayerInputManager pim;
    ActivateHitbox ah;
    HitboxInteractionManager him;
    public ParticleSystem dashps;
    public ParticleSystem permaTrailps;
    public AudioManager am;
    public GameObject sm;
    public EffectManager em;
    InMatchUI imui;
    public AnimationEvents ae;
    TrailRenderer tr;
    PlayerShaderController psc;

    public PhysicsMaterial2D bouncy;
    public PhysicsMaterial2D notBouncy;

    //buffer
    public int FFbufferLength = 10; //how long in seconds an input that is not currently valid will wait to be valid
    int fastFallBuffer = 0;

    //Control Lockers
    public StandardControlLocker grounded;
    public StandardControlLocker hitstun;
    public StandardControlLocker inAnim;
    public StandardControlLocker wallcling;
    public StandardControlLocker onlyAttack;
    public StandardControlLocker dashing;
    public StandardControlLocker inAerialAnim;
    public StandardControlLocker inGrab;
    public StandardControlLocker UNIQUEinMovementAir;
    public StandardControlLocker UNIQUEinMovementGround;
    public StandardControlLocker airborneWithDJ;
    public StandardControlLocker airborneNoDJ;

    public bool touchingGrabbableWall = false;
    public float minimumSpeedForHitstun = 50;
    public int framesInHitstun = 0;
    public bool facingRight = true;
    string currentGroundTag = null;

    [SerializeField] bool spawnSmokeOnDirectionChange;

    //Wall Collision
    public int collidedWallSide;
    public float collidedWallSlope;
    public GameObject platformCollider;

    List<Collider2D> currentOverlaps = new List<Collider2D>();

    //objects
    public GameObject ball;
    public GameObject rune;
    public GameObject counterShield;

    //frames
    public int frame = 0;
    int dropPlatformFrames = 0;
    [SerializeField] int dropPlatformTotalFrames = 7;

    //Animator
    public string currentAnimBool = "FHeavyAttack";

    //debug
    public bool animationDebugMessages = true;

    public List<GameObject> temporaryObjects = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        pim = GetComponent<PlayerInputManager>();
        clm = GetComponent<ControlLockManager>();
        ah = GetComponent<ActivateHitbox>();
        sm = GameObject.Find("SettingsManager");
        am = sm.GetComponent<AudioManager>();
        imui = GetComponent<InMatchUI>();
        ae = GetComponent<AnimationEvents>();
        em = sm.GetComponent<EffectManager>();
        tr = GetComponent<TrailRenderer>();
        psc = GetComponent<PlayerShaderController>();

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        rb.sharedMaterial = notBouncy;

        Physics2D.gravity = Vector2.zero;

        imui.chargeRecoverySpeed = chargeRecoverySpeed;

        AnimatorControllerParameter param;
        for (int index = 0; index < anim.parameters.Length; index++)
        {
            param = anim.GetParameter(index);

            if (param.type == AnimatorControllerParameterType.Bool)
            {
                animatorBools.Add(param.name);
            }
        }
    }

    private void Awake()
    {
        ((IIdentifiable)this).InitializeID();
        GameManager.Instance.TimeController.SubscribeTargetedSlow(this, OnSlow);
    }

    public void OnSlow(float speed)
    {
        if (this == null)
        {
            return;
        }

        anim.speed = speed;

        if (speed == 0)
        {
            beforeFreezeSpeed = rb.velocity;

            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.isKinematic = true;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.isKinematic = false;

            rb.velocity = beforeFreezeSpeed;
        }
    }

    public void FreezeFrames(int framesPerTick, int duration)
    {
        GameManager.Instance.TimeController.Slow(framesPerTick, duration, this);
    }

    void TryBufferFastFall() // Tries to fastfall, if can't fastfall, buffers a fastfall
    {
        if (rb.velocity.y <= 0 && canFastFall)
        {
            StartStopFastFall(true);
        }
        else
        {
            fastFallBuffer = FFbufferLength;
        }
    }

    void StartStopFastFall(bool startstop) // actually starts or stops fastfall
    {
        if (startstop)
        {
            if (applyFFMultiplier == false)
            {
                em.SpawnDirectionalEffect("Sparkle1", new Vector3(transform.position.x + (facingRight ? 1f : -1f), transform.position.y - 1.2f, 0), facingRight);
            }
            applyFFMultiplier = true;
            fastFallBuffer = 0;

            if (rb.velocity.y > -fallSpeed * 0.6f)
            {
                rb.velocity = new Vector2(rb.velocity.x, -fallSpeed * 0.6f);
            }
        }
        else
        {
            applyFFMultiplier = false; 
        }
    }

    void ManageFastFall() // called every frame, manages the buffer for fastfalling
    {
        if (fastFallBuffer > 0 && canFastFall)
        {
            fastFallBuffer -= 1;
            if (rb.velocity.y <= 0)
            {
                StartStopFastFall(true);
            }
        }
        if (delayFF > 0)
        {
            delayFF -= 1;
        }
    }

    public void VerticalResponse(CharacterInput input)
    {
        if (input.Direction.current.y < -0.5)
        {
            platformCollider.SetActive(false);
            if (dropPlatformFrames == 0)
            {
                dropPlatformFrames = 2;
            }

            if (clm.activeLockers.Contains(grounded)) // because this technically triggers on normal ground, not just platforms, edge cases exist
            {
                dropPlatformFrames = dropPlatformTotalFrames;
                if (currentGroundTag == "Platform") // if dropped through a platform, delay the ability to FF for 4 frames
                {
                    delayFF = 5;
                }
            }
            else
            {
                if (delayFF <= 0)
                {
                    if (input.IsPending() || (!clm.activeLockers.Contains(inAnim) && !clm.activeLockers.Contains(inAerialAnim)))
                    {
                        TryBufferFastFall();
                    }
                }
            }
        }
    }

    public void HorizontalResponse(CharacterInput input)
    {
        if (!clm.activeLockers.Contains(UNIQUEinMovementAir) && !clm.activeLockers.Contains(UNIQUEinMovementGround) && !clm.activeLockers.Contains(hitstun))
        {
            if (clm.activeLockers.Contains(grounded)) // if grounded: move
            {
                rb.AddForce(Vector2.right * Mathf.Round(input.Direction.current.x) * groundAccel); // Ground move
            }
            else // if in the air
            {
                if (clm.activeLockers.Contains(inAerialAnim)) // if using an aerial: reduced air movement
                {
                    rb.AddForce(Vector2.right * Mathf.Round(input.Direction.current.x) * airAccelDuringAerial);
                }
                else // if in the air but not using an aerial
                {
                    if (touchingGrabbableWall && !clm.activeLockers.Contains(wallcling)) // if touching wall, if holding toward wall and not using an aerial or already wallcling, grab wall
                    {

                        if (collidedWallSide == Mathf.Sign(pim.GetCurrentDirectional().current.x) && pim.GetCurrentDirectional().current.x != 0)
                        {
                            Debug.Log("here");
                            WallClingEnterExit(true);
                        }
                    }

                    rb.AddForce(Vector2.right * Mathf.Round(input.Direction.current.x) * airAccel); // Air move
                }
            }

            if (clm.activeLockers.Contains(wallcling)) // if wallcling and not holding toward wall, unstick from wall
            {
                if (Mathf.Round(input.Direction.current.x) != collidedWallSide)
                {
                    WallClingEnterExit(false);
                }
            }

            if (!clm.activeLockers.Contains(inAerialAnim))
            {
                Flip(input);
            }
        }
    }

    void HitstunResponse()
    {
        if (animationDebugMessages) { Debug.Log("hitstun response" + "- frame: " + frame); }
        if (rb.velocity.magnitude < minimumSpeedForHitstun && framesInHitstun > minimumHitstunFrames && knockbackTime <= 0 && GameManager.Instance.TimeController.GetTimeScale(this) == 1) // if slow enough and in hitstun for longer than minHitstunFrames and knockback time is over and not in hitstop: stop hitstun
        {
            Hit(null, false);
            framesInHitstun = 0;
            minimumHitstunFrames = 0;
        }
        framesInHitstun += 1;
        if (framesInHitstun == 2)
        {
            firstFrameOfHitstun = false;
        }

        if (knockbackTime <= 0)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, initialLaunchVelocity.magnitude / ((0 - knockbackTime * 5) + 10));
            knockbackTime -= 1;
        }
        else
        {
            knockbackTime -= 1;
        }

        // DI IMPLEMENTATION:
        if (pim.GetCurrentDirectional().current != Vector2.zero)
        {
            float inputAngle = Vector2.Angle(Vector2.right, pim.GetCurrentDirectional().current); // angle of input (0 right, -90 bottom, 90 top)

            if (pim.GetCurrentDirectional().current.y < 0)
            {
                inputAngle *= -1;
            }

            float currentMomentumAngle = Vector2.Angle(Vector2.right, rb.velocity);

            if (rb.velocity.y < 0)
            {
                currentMomentumAngle *= -1;
            }

            currentMomentumAngle = Mathf.MoveTowardsAngle(currentMomentumAngle, inputAngle, DIStrength * moveDiMultiplier);

            rb.velocity = AngleMath.Vector2FromAngle(currentMomentumAngle) * rb.velocity.magnitude;
        }
    }

    void StartKnockback(float knockbackDistance, float knockbackSpeed, Vector2 angle)
    {
        knockbackTime = Mathf.RoundToInt(knockbackDistance / knockbackSpeed);

        initialLaunchVelocity = knockbackSpeed * angle * 100;
        rb.velocity = initialLaunchVelocity;
        
    }

    public void UseRune()
    {
        if (runeActive == null)
        {
            runeActive = Instantiate(rune, transform.position, Quaternion.identity);
        }
        else
        {
            transform.position = runeActive.transform.position;
            Destroy(runeActive);
        }
    }

    public void DashResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            if (imui.currentCharge >= dashCost)
            {
                if (animationDebugMessages) { Debug.Log("Dash started" + "- frame: " + frame); }
                ae.StopAnimation(currentAnimBool);
                ae.ChangeAnimBool("StartDash", true);
                queuedKnockback = null;
                clm.AddLocker(dashing);

                rb.velocity = Vector2.zero;
                imui.ChangeCharge(-dashCost);
            }
            else
            {
                // sound effect for trying to dash with no charge goes here
            }
        }
    }

    public void FLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            if (animationDebugMessages) { Debug.Log("FTilt Response" + "- frame: " + frame); }
            Flip(input);

            ae.ChangeAnimBool("FLightAttack", true);
        }
    }

    public void NeutralAttackResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            if (animationDebugMessages) { Debug.Log("FTilt Response" + "- frame: " + frame); }
            Flip(input);

            ae.ChangeAnimBool("FLightAttack", true);
        }
    }

    public void UpLightResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("UpTilt Response" + "- frame: " + frame); }
        if (input.IsHeld() || input.IsPending())
        {
            ae.ChangeAnimBool("UpLightAttack", true);
        }
    }

    public void DLightResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("DLight Response" + "- frame: " + frame); }
        if (input.IsHeld() || input.IsPending())
        {
            if (DLightBounce)
            {
                if (clm.activeLockers.Contains(grounded))
                {
                    delayFF = 10;
                    rb.AddForce(150 * Vector2.up);
                }
            }

            ae.ChangeAnimBool("DLightAttack", true);

            fastFallBuffer = 0;
        }
    }

    public void UpHeavyResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("UpHeavy Response" + "- frame: " + frame); }
        if (input.IsHeld() || input.IsPending())
        {
            Flip(input);

            ae.ChangeAnimBool("UpHeavyAttack", true);
        }
    }

    public void FHeavyResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("FHeavy Response" + "- frame: " + frame); }
        if (input.IsHeld() || input.IsPending())
        {
            ae.ChangeAnimBool("FHeavyAttack", true);
        }
    }

    public void DHeavyResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("DHeavy Response" + "- frame: " + frame); }
        if (input.IsHeld() || input.IsPending())
        {
            ae.ChangeAnimBool("DHeavyAttack", true);

            fastFallBuffer = 0;
        }
    }

    public void JumpResponse(CharacterInput input)
    {
        if (animationDebugMessages) { Debug.Log("Jump Response" + "- frame: " + frame); }
        pim.CacheInput(input);

        if (clm.activeLockers.Contains(grounded))
        {
            ae.ChangeAnimBool("Jumpsquat", true);
        }
        else if (clm.activeLockers.Contains(wallcling))
        {
            ae.ChangeAnimBool("WallJumpSquat", true);
        }
        else if (clm.activeLockers.Contains(airborneWithDJ))
        {
            DoubleJump();
        }

        void DoubleJump()
        {
            clm.RemoveLocker(airborneWithDJ);
            clm.AddLocker(airborneNoDJ);
            canDoubleJump = false;
            StartStopFastFall(false);
            Debug.Log("ffdelay set to 5");
            delayFF = 5;
            em.SpawnDirectionalEffect("DoubleJumpRing", transform.position, facingRight);

            rb.velocity = new Vector2(rb.velocity.x, 0.001f);
            rb.AddForce(initialJumpForce * Vector2.up * doubleJumpMultiplier);
        }
    }

    public void MovementResponse(CharacterInput input)
    {
        pim.CacheInput(input);

        if (input.IsHeld() || input.IsPending())
        {
            ae.ChangeAnimBool("Movement", true);
        }
    }

    public void SpecialResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            ae.ChangeAnimBool("Special", true);
        }
    }

    public void WallClingEnterExit(bool enterexit)
    {
        if (enterexit == clm.activeLockers.Contains(wallcling))
        {
            return;
        }

        if (enterexit)
        {
            clm.AddLocker(wallcling);
            ae.ChangeAnimBool("WallCling", true);
            canDoubleJump = true;
            clm.RemoveLocker(airborneNoDJ);
            clm.RemoveLocker(airborneWithDJ);
            affectedByGravity = false;
            rb.velocity = Vector2.zero;
        }
        else
        {
            clm.RemoveLocker(wallcling);
            if (canDoubleJump)
            {
                clm.AddLocker(airborneWithDJ);
            }
            else
            {
                clm.AddLocker(airborneNoDJ);
            }

            affectedByGravity = true;
            ae.ChangeAnimBool("WallCling", false);
        }
    }

    void OnDirectionChange()
    {
        if (spawnSmokeOnDirectionChange)
        {
            if (clm.activeLockers.Contains(grounded))
            {
                ae.SpawnDirectionalSmokeCloud();
            }
        }
    }

    void Flip(CharacterInput input)
    {
        if (Mathf.Round(input.Direction.current.x) > 0)
        {
            if (!facingRight)
            {
                facingRight = true;
                sr.flipX = false;
                OnDirectionChange();
            }
        }
        else if (Mathf.Round(input.Direction.current.x) < 0)
        {
            if (facingRight)
            {
                facingRight = false;
                sr.flipX = true;
                OnDirectionChange();
            }
        }

        bc.offset = new Vector2(0.254f * (facingRight ? -1 : 1), bc.offset.y);
    }

    public void ChangeIntangible(bool enterexit)
    {
        if (enterexit)
        {
            Debug.Log("intangible");
            intangible = true;
        }
        else
        {
            intangible = false;
        }
    }

    private void FixedUpdate()
    {
        anim.SetFloat("VerticalInput", pim.GetCurrentDirectional().current.y);
        anim.SetFloat("HorizontalInput", pim.GetCurrentDirectional().current.x);

        anim.SetFloat("VerticalVelocity", rb.velocity.y);
        anim.SetFloat("HorizontalVelocity", rb.velocity.x);

        if (clm.activeLockers.Contains(hitstun))
        {
            HitstunResponse(); // if in hitstun, once per frame, check moving slow enough that hitstun is over
        }

        frame += 1;
        if (frame > 60) { frame = 1; }

        if (!doNotUpdatePlatformCollider)
        {
            ManagePlatformCollider();
        }

        ManageForces();

        UpdateGrounded();

        ManageQueuedKnockback();

        ManageFastFall();

        if (psc != null)
        {
            if (psc.ShaderStrength > 0)
            {
                psc.ShaderStrength -= 0.1f;
            }
        }

        if (!affectedByGravity && !clm.activeLockers.Contains(dashing) && !clm.activeLockers.Contains(UNIQUEinMovementAir) && !clm.activeLockers.Contains(UNIQUEinMovementGround))
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }
    }

    void ManagePlatformCollider()
    {
        if (clm.activeLockers.Contains(hitstun))
        {
            platformCollider.SetActive(false);
        }
        else
        {
            if (dropPlatformFrames <= 0)
            {
                platformCollider.SetActive(true);
                return;
            }
        }

        if (dropPlatformFrames > 0)
        {
            dropPlatformFrames -= 1;
        }
    }

    void ManageForces()
    {
        if (!clm.activeLockers.Contains(hitstun))
        {
            if (clm.activeLockers.Contains(grounded)) // if grounded + not in hitstun
            {
                if (Mathf.Round(pim.GetCurrentDirectional().current.x) != Mathf.Sign(rb.velocity.x) || !clm.ControlsAllowed(ControlLock.Controls.HORIZONTAL)
                    || (Mathf.Round(pim.GetCurrentDirectional().current.x) == 0)) //if grounded and: can't move or not holding the direction of motion: apply friction
                {
                    if (!ignoreFriction)
                    {
                        if (Mathf.Abs(rb.velocity.x) >= friction) //if speed is greater than friction
                        {
                            rb.velocity -= new Vector2(Mathf.Sign(rb.velocity.x) * friction, 0); //reduce velocity by friction
                        }
                        else
                        {
                            rb.velocity = new Vector2(0, rb.velocity.y); //if speed is < friction, set speed to 0
                        }
                    }
                }

                if (!clm.activeLockers.Contains(dashing) && !ignoreFriction)
                {
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y); // cap x speed
                }
            }
            else // if not grounded + not in hitstun
            {
                if (Mathf.Round(pim.GetCurrentDirectional().current.x) != Mathf.Sign(rb.velocity.x) || (Mathf.Round(pim.GetCurrentDirectional().current.x) == 0))
                //if not in hitstun, airborne, and not holding the direction of motion: apply airFriction
                {
                    if (!ignoreFriction)
                    {
                        if (Mathf.Abs(rb.velocity.x) >= airFriction) //if speed is greater than airFriction
                        {
                            rb.velocity -= new Vector2(Mathf.Sign(rb.velocity.x) * airFriction, 0); //reduce velocity by airFriction
                        }
                        else
                        {
                            rb.velocity = new Vector2(0, rb.velocity.y); //if speed is < friction, set speed to 0
                        }
                    }
                }

                if (!clm.activeLockers.Contains(wallcling) && affectedByGravity) // if not in hitstun, wallclinging, or ignoreGravity
                {
                    rb.AddForce(Vector2.down * fallAccel * (applyFFMultiplier ? fastFallMultiplier : 1)); // Apply gravity
                }

                if (!ignoreFriction)
                {
                    float newXSpeed = rb.velocity.x;
                    float newYSpeed = rb.velocity.y;
                    // slow down toward max speeds
                    if (Mathf.Abs(rb.velocity.x) > airSpeed)
                    {
                        newXSpeed = Mathf.MoveTowards(rb.velocity.x, airSpeed * Mathf.Sign(rb.velocity.x), overMaxXSpeedAdjustment);
                    }
                    if (rb.velocity.y < -fallSpeed)
                    {
                        newYSpeed = Mathf.MoveTowards(rb.velocity.y, -fallSpeed, overMaxYSpeedAdjustment);
                    }

                    rb.velocity = new Vector2(newXSpeed, newYSpeed);
                }

            }
        }
        else // if in hitstun
        {
            if (!clm.activeLockers.Contains(wallcling) && affectedByGravity) // if not wallclinging or ignoreGravity
            {
                rb.AddForce(Vector2.down * (fallAccel / 2)); // Apply gravity BUT HALVED, BECAUSE YOU'RE IN HITSTUN (should it be the same gravity?)
            }
        }
    }

    // Queued knockback is not executed if inGrab or if in hitstop
    void ManageQueuedKnockback()
    {
        if (queuedKnockback != null && !clm.activeLockers.Contains(inGrab))
        {
            if (GameManager.Instance.TimeController.GetTimeScale(this) == 1)
            {
                StartKnockback(queuedKnockback.left.kbDistance, queuedKnockback.left.kbSpeedMultiplier, queuedKnockback.right);
                queuedKnockback = null;
            }
        }
    }

    // Updates grounded condition every frame.
    void UpdateGrounded()
    {
        foreach (Collider2D i in currentOverlaps) // for all colliders the player is currently touching
        {
            if (i.CompareTag("Ground") || i.CompareTag("Platform")) // if one is ground/platform
            {
                if (!clm.activeLockers.Contains(grounded)) // if not grounded
                {
                    if (rb.velocity.y <= 0 && GameManager.Instance.TimeController.GetTimeScale(this) != 0) // if moving downward (and not frozen), become grounded
                    {
                        BecomeGrounded(i.tag);
                    }
                    else if (i.gameObject.layer != 6) // physics layer 6 is platforms. This WILL break if anyone rearranges the layers
                    {
                        BecomeGrounded(i.tag); // if moving upward, but collision is not a platform, become grounded anyway. This breaks if we have standable walls
                    }
                }
                return;
            }
        }
        if (clm.activeLockers.Contains(grounded))
        {
            BecomeGrounded(null, false); // it would've returned if any current collisions ground/platform so must not be on the ground
        }

        void BecomeGrounded(string groundTag, bool enterexit = true)
        {
            if (enterexit)
            {
                clm.AddLocker(grounded);
                clm.RemoveLocker(airborneWithDJ);
                clm.RemoveLocker(airborneNoDJ);

                canDoubleJump = true;

                ae.ChangeAnimBool("Grounded", true);
                applyFFMultiplier = false;
                fastFallBuffer = 0;
                currentGroundTag = groundTag;

                if (anim.GetBool("Movement"))
                {
                    clm.RemoveLocker(UNIQUEinMovementAir);
                    clm.AddLocker(UNIQUEinMovementGround);
                }
                else if (anim.GetBool("Special"))
                {
                    clm.RemoveLocker(inAerialAnim);
                    clm.AddLocker(inAnim);
                }

                if (!clm.activeLockers.Contains(hitstun))
                {
                    gameObject.layer = 9;
                }
            }
            else
            {
                clm.RemoveLocker(grounded);
                clm.AddLocker(airborneWithDJ);
                currentGroundTag = null;
                ae.ChangeAnimBool("Grounded", false);
                gameObject.layer = 8;
            }
        }
    }

    // Starts hitstun and queues knockback or stops hitstun
    public void Hit(Collider2D collider, bool enterOrExitHitstun = true)
    {
        if (enterOrExitHitstun) // Deal with being hit:
        {
            if (!clm.activeLockers.Contains(inGrab))
            {
                StopEverything();
            }
            else
            {
                StopEverything(false);
            }

            clm.AddLocker(hitstun);
            ae.ChangeAnimBool("Hitstun", true);

            gameObject.layer = 8;
            rb.sharedMaterial = bouncy;

            HitboxInfo hi = collider.gameObject.GetComponent<HitboxInfo>();
            moveDiMultiplier = hi.DiMultiplier;

            framesInHitstun = 0;
            firstFrameOfHitstun = true;

            em.SpawnHitEffectOnContactPoint("HitExplosion1", collider, bc.bounds.center);

            minimumHitstunFrames = hi.minimumHitstunFrames;

            tr.emitting = true;

            if (psc != null)
            {
                psc.ShaderStrength = 1;
            }

            QueueKnockback();

            imui.ChangeHealth(-hi.damage);

            void QueueKnockback()
            {
                if (hi.angle == 361)
                {
                    int facingRightInt = hi.facingRight ? 1 : -1;

                    Vector3 hiParentPosition = hi.transform.root.position;

                    Vector2 goalPosition = new Vector2(hiParentPosition.x + (hi.knockbackGoalPos.x * facingRightInt), hiParentPosition.y + hi.knockbackGoalPos.y);
                    Vector2 between = new Vector2(goalPosition.x - transform.position.x, goalPosition.y - transform.position.y);

                    queuedKnockback = new Pair<HitboxInfo, Vector2>(hi, between);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    Vector2 angleOfForce = AngleMath.Vector2FromAngle(hi.angle);

                    if (!hi.facingRight)
                    {
                        angleOfForce.x *= -1;
                    }

                    angleOfForce = angleOfForce.normalized;

                    queuedKnockback = new Pair<HitboxInfo, Vector2>(hi, angleOfForce);
                }
            }

        }
        else // Stop hitstun:
        {
            ae.StopAnimation(currentAnimBool);

            if (clm.activeLockers.Contains(grounded))
            {
                gameObject.layer = 9;
            }

            moveDiMultiplier = 1;

            rb.sharedMaterial = notBouncy;

            tr.emitting = false;

            ae.ChangeAnimBool("Hitstun", false);
        }
    }

    // This systematically stops EVERYTHING the player could be doing at a given time
    public void StopEverything(bool exitGrab = true)
    {
        List<string> trueBoolsCopy = new List<string>(ae.trueAnimBools);
        foreach (string i in trueBoolsCopy) // set all bools false
        {
            if (i != "Grounded" && i != "Airborne")
            {
                ae.ChangeAnimBool(i, false);
            }
        }

        if (exitGrab) // remove all lockers except grounded and airborne
        {
            clmEx.RemoveAllLockersExcept(clm, new StandardControlLocker[] { grounded, airborneWithDJ, airborneNoDJ });
        }
        else // don't remove grab if it's 
        {
            clmEx.RemoveAllLockersExcept(clm, new StandardControlLocker[] { grounded, airborneWithDJ, airborneNoDJ, inGrab });
        }

        foreach(GameObject i in temporaryObjects)
        {
            Destroy(i);
        }
        temporaryObjects.Clear();

        moveDiMultiplier = 1;

        rb.sharedMaterial = notBouncy;

        tr.emitting = false;

        ChangeIntangible(false);

        ignoreFriction = false;
        affectedByGravity = true;
        doNotUpdatePlatformCollider = false;
        canFastFall = true;

        ae.UnfreezePlayer();

        if (permaTrailps != null)
        {
            permaTrailps.Play();
        }

        canFastFall = true;

        ResetDoNotEnable();
    }

    public void ResetDoNotEnable()
    {
        List<GameObject> dneCopy = new List<GameObject>(him.doNotEnableHitboxes);
        foreach (GameObject i in dneCopy)
        {
            if (i != null)
            {
                i.GetComponent<HitboxInfo>().doNotEnable = false;
            }
        }
        him.doNotEnableHitboxes.Clear();
    }

    public void Grabbed(Collider2D collider)
    {
        imui.ChangeHealth(-collider.GetComponent<HitboxInfo>().damage);
        ae.StopAnimation(currentAnimBool);

        clm.AddLocker(inGrab);
        ae.ChangeAnimBool("Hitstun", true);
    }

    public void ExitGrab()
    {
        if (queuedKnockback == null)    // if player was not hit during the grab, releases fully;
        {
            Debug.Log("no queued knockback, exited grab frame: " + frame);
            Hit(null, false);
        }
        else    // otherwise just disables grab locker so queued knockback will execute the knockback
        {
            clm.RemoveLocker(inGrab);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            collidedWallSide = (int)Mathf.Sign(collision.GetContact(0).point.x - transform.position.x);
        }

        if (!currentOverlaps.Contains(collision.collider))
        {
            currentOverlaps.Add(collision.collider);
        }

        if (clm.activeLockers.Contains(hitstun) && collision.gameObject.CompareTag("Wall"))
        {
            am.PlaySound("Impact1");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            if (collision.GetContact(0).normal.y == 0)
            {
                touchingGrabbableWall = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            collidedWallSide = 0;
            touchingGrabbableWall = false;
        }

        if (currentOverlaps.Contains(collision.collider)) { currentOverlaps.Remove(collision.collider); };
    }
}
