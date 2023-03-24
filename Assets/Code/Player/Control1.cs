using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System.Drawing;
using System.Linq;
using UnityEngine.TextCore.Text;

public class Control1 : MonoBehaviour, IIdentifiable
{
    // Todo list
    // Fix dashes, which were completely broken by friction changes
    // Implement something in StopLandingLag to actually stop the freeze, right now it does nothing

    private uint id;
    private bool initializedID;
    bool IIdentifiable.InitializedID { get { return initializedID; } set { initializedID = value; } }
    uint IIdentifiable.ID { get { return id; } set { id = value; } }

    public GameObject testCircle;
    public Vector3 feetOffset;
    float minimumHitstunFrames = 0;

    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;
    [SerializeField] float airFriction = 500;
    public float wallJumpVerticalOoOne = 0.5f;
    public bool ignoreGravity = false;
    public bool intangible = false;
    public bool ignoreFriction = false;
    [SerializeField] float DIStrength = 5;

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
    ControlLockManager clm;
    PlayerInputManager pim;
    ActivateHitbox ah;
    HitboxInteractionManager him;
    ParticleSystem trailps;
    public AudioManager am;
    public GameObject sm;
    EffectManager em;
    InMatchUI imui;
    AnimationEvents ae;
    TrailRenderer tr;
    PlayerShaderController psc;

    public PhysicsMaterial2D bouncy;
    public PhysicsMaterial2D notBouncy;

    //buffer
    public int bufferLength = 5; //how long in seconds an input that is not currently valid will wait to be valid

    //Control Lockers
    public StandardControlLocker grounded;
    public StandardControlLocker airborne;
    public StandardControlLocker hitstun;
    public StandardControlLocker inAnim;
    public StandardControlLocker wallcling;
    public StandardControlLocker onlyAttack;
    public StandardControlLocker dashing;
    public StandardControlLocker inAerialAnim;

    public Collider2D wallTouching;
    public bool touchingWall = false;
    public float minimumSpeedForHitstun = 50;
    public int framesInHitstun = 0;
    public bool facingRight = true;

    [SerializeField] bool spawnSmokeOnDirectionChange;

    //Wall Collision
    public int collidedWallSide;
    public float collidedWallSlope;
    [SerializeField] GameObject platformCollider;

    List<Collider2D> currentOverlaps = new List<Collider2D>();

    //objects
    public GameObject ball;

    //frames
    public int frame = 0;
    int dropPlatformFrames = 0;
    [SerializeField] int dropPlatformTotalFrames = 7;

    //Animator
    public string currentAnimBool = "FHeavyAttack";

    //debug
    public bool animationDebugMessages = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        pim = GetComponent<PlayerInputManager>();
        clm = GetComponent<ControlLockManager>();
        ah = GetComponent<ActivateHitbox>();
        trailps = GetComponent<ParticleSystem>();
        sm = GameObject.Find("SettingsManager");
        am = sm.GetComponent<AudioManager>();
        imui = GetComponent<InMatchUI>();
        ae = GetComponent<AnimationEvents>();
        em = sm.GetComponent<EffectManager>();
        tr = GetComponent<TrailRenderer>();
        psc = GetComponent<PlayerShaderController>();

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        rb.sharedMaterial = notBouncy;
    }

    private void Awake()
    {
        ((IIdentifiable)this).InitializeID();
        GameManager.Instance.TimeController.SubscribeTargetedSlow(this, OnSlow);
    }

    public void OnSlow(float speed)
    {
        anim.speed = speed;
        rb.simulated = speed != 0;
        rb.gravityScale = speed;
    }

    public void FreezeFrames(int framesPerTick, int duration, IIdentifiable identifiable)
    {
        GameManager.Instance.TimeController.Slow(framesPerTick, duration, identifiable);
    }

    public void VerticalResponse(CharacterInput input)
    {
        if (input.Direction.current.y < -0.5)
        {
            platformCollider.SetActive(false);
            if (dropPlatformFrames == 0)
            {
                dropPlatformFrames = 1;
            }

            if (clm.activeLockers.Contains(grounded))
            {
                dropPlatformFrames = dropPlatformTotalFrames;
            }
        }
    }

    public void HorizontalResponse(CharacterInput input)
    {
        if (!clm.activeLockers.Contains(hitstun))
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
                    if (touchingWall) // if touching wall, if holding toward wall and not using an aerial, grab wall
                    {
                        if (collidedWallSide == pim.GetCurrentDirectional().current.x)
                        {
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
        else // DI Implementation
        {
            float angleOfMotion = Vector2.Angle(Vector2.right, new Vector2(Mathf.Abs(rb.velocity.x), rb.velocity.y)); // angle difference from 0 or 180; angle of 200 returns 20
            if (angleOfMotion >= 45)
            {
                if (rb.velocity.y > 0)
                {
                    rb.AddForce(new Vector2(input.Direction.current.x * DIStrength, -input.Direction.current.x * (DIStrength/5)));
                }
                else
                {
                    rb.AddForce(new Vector2(input.Direction.current.x * DIStrength, input.Direction.current.x * (DIStrength/5)));
                }
            }
            else
            {
                if (rb.velocity.x > 0)
                {
                    rb.AddForce(new Vector2(input.Direction.current.x * (DIStrength/5), -input.Direction.current.x * DIStrength));
                    rb.AddForce(new Vector2(-input.Direction.current.y * (DIStrength / 5), input.Direction.current.y * DIStrength));
                }
                else
                {
                    rb.AddForce(new Vector2(input.Direction.current.x * (DIStrength/5), input.Direction.current.x * DIStrength));
                    rb.AddForce(new Vector2(input.Direction.current.y * (DIStrength / 5), input.Direction.current.y * DIStrength));
                }
            }
        }


    }

    void HitstunResponse()
    {
        if (animationDebugMessages) { Debug.Log("hitstun response" + "- frame: " + frame); }

        if (rb.velocity.magnitude < minimumSpeedForHitstun && framesInHitstun > 10)
        {
            Hit(null, false);
            framesInHitstun = 0;
            minimumHitstunFrames = 0;
        }
        framesInHitstun += 1;
    }

    public void DashResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            if (imui.currentCharge >= dashCost)
            {
                if (animationDebugMessages) { Debug.Log("Dash started" + "- frame: " + frame); }
                if (currentAnimBool != null)
                {
                    ae.StopAnimation(currentAnimBool);
                }
                ae.ChangeAnimBool("StartDash", true);
                ae.ChangeAnimBool("StopDash", true); // not entirely sure if this line is even necessary after I added ExitTime
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
            if (clm.activeLockers.Contains(grounded))
            {
                rb.AddForce(120 * Vector2.up);
            }


            ae.ChangeAnimBool("DLightAttack", true);
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
    }

    void WallClingEnterExit(bool enterexit)
    {
        if (enterexit)
        {
            clm.AddLocker(wallcling);
            anim.SetBool("WallCling", true);
            rb.velocity = Vector2.zero;
        }
        else
        {
            clm.RemoveLocker(wallcling);
            anim.SetBool("WallCling", false);
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

    private void FixedUpdate()
    {
        anim.SetFloat("VerticalInput", pim.GetCurrentDirectional().current.y);
        anim.SetFloat("HorizontalInput", pim.GetCurrentDirectional().current.x);

        anim.SetFloat("VerticalVelocity", rb.velocity.y);
        anim.SetFloat("HorizontalVelocity", rb.velocity.x);

        frame += 1;
        if (frame > 60) { frame = 1; }

        if (clm.activeLockers.Contains(wallcling)) // This is an absolutely disgusting thing to have to run, I hope I can change this.
                                                   // Sets speed to 0 every frame while wall-clinging
            // The reason this is called at all is because stopping all momentum on the frame I grab the wall sometimes just doesn't work
        {
            rb.velocity = Vector2.zero;
        }

        if (clm.activeLockers.Contains(hitstun))
        {
            HitstunResponse(); // if in hitstun, once per frame, check moving slow enough that hitstun is over
        }

        ManageForces();

        UpdateGrounded();

        ManagePlatformCollider();

        if (psc != null)
        {
            if (psc.ShaderStrength > 0)
            {
                psc.ShaderStrength -= 0.1f;
            }
        }

    }

    void ManagePlatformCollider()
    {
        if (dropPlatformFrames == 0)
        {
            platformCollider.SetActive(true);
            return;
        }
        dropPlatformFrames -= 1;
    }

    void ManageForces()
    {
        if (clm.activeLockers.Contains(grounded)) // if grounded
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

            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y); // cap x speed
        }
        else // if not grounded
        {
            if (!clm.activeLockers.Contains(hitstun)) // if not grounded and not in hitstun
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

                if (!clm.activeLockers.Contains(wallcling) && !ignoreGravity) // if not in hitstun, wallclinging, or ignoreGravity
                {
                    rb.AddForce(Vector2.down * fallAccel); // Apply gravity
                }

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
            else // if not grounded and you ARE in hitstun
            {
                if (!clm.activeLockers.Contains(wallcling) && !ignoreGravity) // if not wallclinging or ignoreGravity
                {
                    rb.AddForce(Vector2.down * (fallAccel / 2)); // Apply gravity BUT HALVED, BECAUSE YOU'RE IN HITSTUN (should it be the same gravity?)
                }
            }
        }
    }

    // Updates grounded condition every frame.
    void UpdateGrounded()
    {
        foreach (Collider2D i in currentOverlaps) // for all colliders the player is currently touching
        {
            if (i.CompareTag("Standable")) // if one is standable
            {
                if (!clm.activeLockers.Contains(grounded)) // if not grounded
                {
                    if (rb.velocity.y <= 0) // if moving downward, just be grounded no matter what
                    {
                        BecomeGrounded();
                    }
                    else if (i.gameObject.layer != 6) // physics layer 6 is platforms. This WILL break if anyone rearranges the layers
                    {
                        BecomeGrounded(); // if moving upward, but collision is not a platform, become grounded anyway, because some weird shit happened
                    }
                }
                return;
            }
        }
        BecomeGrounded(false); // it would've returned if any current collisions standable so must not be on the ground

        void BecomeGrounded(bool enterexit = true)
        {
            if (enterexit)
            {
                clm.AddLocker(grounded);
                anim.SetBool("Grounded", true);
                if (!clm.activeLockers.Contains(hitstun))
                {
                    gameObject.layer = 9;
                }
            }
            else
            {
                clm.RemoveLocker(grounded);
                anim.SetBool("Grounded", false);
                gameObject.layer = 8;
            }
        }
    }

    public void Hit(Collider2D collider, bool enterOrExitHitstun = true)
    {
        if (enterOrExitHitstun)
        {
            clm.AddLocker(hitstun);
            gameObject.layer = 8;
            rb.sharedMaterial = bouncy;

            HitboxInfo hi = collider.gameObject.GetComponent<HitboxInfo>();
            anim.SetBool("Hitstun", true);

            em.SpawnHitEffectOnContactPoint("HitExplosion1", collider, bc.bounds.center);

            minimumHitstunFrames = hi.minimumHitstunFrames;

            tr.emitting = true;

            if (psc != null)
            {
                psc.ShaderStrength = 1;
            }


            if (hi.angle == 361)
            {
                Vector2 hiParentVelocity = hi.transform.root.GetComponent<Rigidbody2D>().velocity;
                Vector3 hiParentPosition = hi.transform.root.position;

                Vector2 goalPosition = new Vector2(hiParentPosition.x + (hi.facingRight?1:-1), hiParentPosition.y);
                Vector2 between = new Vector2(goalPosition.x - transform.position.x, goalPosition.y - transform.position.y);
                rb.AddForce(((between + (hiParentVelocity / 9)) * hi.knockback));
            }
            else if (hi.angle == 362)
            {
                Vector2 hiParentVelocity = hi.transform.root.GetComponent<Rigidbody2D>().velocity;
                Vector3 hiParentPosition = hi.transform.root.position;

                Vector2 goalPosition = new Vector2(hiParentPosition.x, hiParentPosition.y + 2f);
                Vector2 between = new Vector2(goalPosition.x - transform.position.x, goalPosition.y - transform.position.y);
                rb.AddForce(((between + (hiParentVelocity / 9)) * hi.knockback));
            }
            else
            {
                rb.velocity = Vector2.zero;
                imui.ChangeHealth(-hi.damage);
                Vector2 angleOfForce = AngleMath.Vector2FromAngle(hi.angle);

                if (!hi.facingRight)
                {
                    angleOfForce.x *= -1;
                }

                if (!hi.angleIndependentOfMovement)
                {
                    Vector2 objectVelocity = hi.owner.GetComponent<Rigidbody2D>().velocity;
                    angleOfForce = (angleOfForce.normalized + objectVelocity.normalized).normalized;
                }
                else
                {
                    angleOfForce = angleOfForce.normalized;
                }

                rb.AddForce(angleOfForce * hi.knockback);
            }

        }
        else
        {
            clm.RemoveLocker(hitstun);
            if (clm.activeLockers.Contains(grounded))
            {
                gameObject.layer = 9;
            }
            rb.sharedMaterial = notBouncy;
            tr.emitting = false;
            anim.SetBool("Hitstun", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            collidedWallSide = (int)Mathf.Sign(collision.GetContact(0).point.x - transform.position.x);
            touchingWall = true;
            wallTouching = collision.collider;
        }

        if (!currentOverlaps.Contains(collision.collider))
        {
            currentOverlaps.Add(collision.collider);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out HitboxInfo _))
        {
            if (!him.triggersThisFrame.Contains(collision))
            {
                him.triggersThisFrame.Add(collision);
            }
            if (!him.triggersThisFrame.Contains(bc))
            {
                him.triggersThisFrame.Add(bc);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            WallClingEnterExit(false);
            collidedWallSide = 0;
            touchingWall = false;
            wallTouching = null;
        }

        if (currentOverlaps.Contains(collision.collider)) { currentOverlaps.Remove(collision.collider); };
    }
}
