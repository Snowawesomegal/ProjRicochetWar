using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System.Drawing;
using System.Linq;

public class Control1 : MonoBehaviour
{
    //March 3, 2023, 6:00PM

    public GameObject testCircle;

    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;
    public float wallJumpVerticalOoOne = 0.5f;
    public bool ignoreGravity = false;
    public bool intangible = false;

    //speeds
    public float airSpeed = 10;
    [SerializeField] float airAccel = 10;
    public float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    public float initialJumpForce = 10;
    public float dashForce = 10;

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
    GameObject sm;
    InMatchUI imui;
    AnimationEvents ae;

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

    public Collider2D wallTouching;
    public bool touchingWall = false;
    public float minimumSpeedForHitstun = 50;
    public int framesInHitstun = 0;
    public bool facingRight = true;

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

    //Animator
    public string currentAnimBool;
    string[] attackBools = new string[] { "FLightAttack", "FHeavyAttack", "UpLightAttack", "UpHeavyAttack", "FAirAttack", "UpAirAttack", "DAirAttack", "BAirAttack" };

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

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        rb.sharedMaterial = notBouncy;

        friction /= 100; //adjusting friction to make it smaller because friction's effect is massive
    }

    public void VerticalResponse(CharacterInput input)
    {
        if (input.Direction.current.y < -0.5)
        {
            platformCollider.SetActive(false);
            if (clm.activeLockers.Contains(grounded))
            {
                dropPlatformFrames = 0; //implement this doing something
            }
        }
        else
        {
            platformCollider.SetActive(true);
        }
    }

    public void HorizontalResponse(CharacterInput input)
    {
        if (clm.activeLockers.Contains(wallcling))
        {
            if (Mathf.Round(input.Direction.current.x) != collidedWallSide)
            {
                clm.RemoveLocker(wallcling);
            }
        }
        else
        {
            if (clm.activeLockers.Contains(grounded))
            {
                rb.AddForce(Vector2.right * Mathf.Round(input.Direction.current.x) * groundAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y);
                //above line caps horizontal speed at groundspeed every frame
                //this is a placeholder- placing a hard cap on horizontal speed fundamentally restricts future movement and must be changed
                //best solution is to constantly reduce speed until speed is within desired parameters while touching ground and not in hitstun
            }
            else
            {
                if (touchingWall)
                {
                    if (collidedWallSide == pim.GetCurrentDirectional().current.x)
                    {
                        clm.AddLocker(wallcling);
                        rb.velocity = Vector2.zero;
                    }
                }

                rb.AddForce(Vector2.right * Mathf.Round(input.Direction.current.x) * airAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -airSpeed, airSpeed), rb.velocity.y);
                //same as grounded
            }

            Flip(input);
        }
    }

    void HitstunResponse()
    {
        if (rb.velocity.magnitude < minimumSpeedForHitstun && framesInHitstun > 10)
        {
            Hit(null, false);
            framesInHitstun = 0;
        }
        framesInHitstun += 1;
    }

    public void DashResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            if (imui.currentCharge >= dashCost)
            {
                if (currentAnimBool != null)
                {
                    ae.StopAnimation(currentAnimBool);
                }
                ChangeAnimBool("StartDash", true);
                ChangeAnimBool("StopDash", true); //Dash ends when StopDash is false
                clm.AddLocker(dashing);

                rb.velocity = Vector2.zero;
                imui.ChangeCharge(-dashCost);
            }
            else
            {
                // sound effect for no dash charge goes here
            }
        }

    }

    public void FLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            Flip(input);

            ChangeAnimBool("FLightAttack", true);
        }
    }

    public void UpLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            ChangeAnimBool("UpLightAttack", true);
        }
    }

    public void DownLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            ChangeAnimBool("DownLightAttack", true);
        }
    }

    public void UpHeavyResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            ChangeAnimBool("UpHeavyAttack", true);
        }
    }

    public void JumpResponse(CharacterInput input)
    {
        pim.CacheInput(input);

        if (clm.activeLockers.Contains(grounded))
        {
            ChangeAnimBool("Jumpsquat", true);
        }
        else if (clm.activeLockers.Contains(wallcling))
        {
            ChangeAnimBool("WallJumpSquat", true);
        }
    }

    public void ChangeAnimBool(string boolName, bool toSet)
    {
        anim.SetBool(boolName, toSet);
        if (toSet == true)
        {
            if (attackBools.Contains(boolName))
            {
                currentAnimBool = boolName;
            }
        }
        else
        {
            currentAnimBool = null;
        }
    }

    void Flip(CharacterInput input)
    {
        if (Mathf.Round(input.Direction.current.x) > 0)
        {
            facingRight = true;
            sr.flipX = false;
        }
        else if (Mathf.Round(input.Direction.current.x) < 0)
        {
            facingRight = false;
            sr.flipX = true;
        }

        bc.offset = new Vector2(0.254f * (facingRight ? -1 : 1), bc.offset.y);
    }

    private void FixedUpdate()
    {
        frame += 1;
        if (frame > 60) { frame = 1; }

        if (clm.activeLockers.Contains(wallcling)) // This is an absolutely disgusting thing to have to run, I hope I can change this
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
    }

    void ManageForces()
    {
        if (clm.activeLockers.Contains(grounded))
        {
            if (Mathf.Round(pim.GetCurrentDirectional().current.x) != Mathf.Sign(rb.velocity.x) || !clm.ControlsAllowed(ControlLock.Controls.HORIZONTAL)
                || (Mathf.Round(pim.GetCurrentDirectional().current.x) == 0)) //if grounded/can't move/not holding the direction of motion;
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
        else
        {
            if (!clm.activeLockers.Contains(wallcling) && !ignoreGravity)
            {
                rb.AddForce(Vector2.down * fallAccel);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -fallSpeed, 999999));
                //this is a placeholder, see directional response's explanation below
            }

        }
    }

    // Updates grounded condition every frame.
    void UpdateGrounded()
    {
        foreach (Collider2D i in currentOverlaps) // for all colliders the player is currently touching
        {
            Debug.Log(i.gameObject.name);
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


            if (hi.angle == 361)
            {
                Vector2 hiParentVelocity = hi.transform.parent.GetComponent<Rigidbody2D>().velocity;
                Vector3 hiParentPosition = hi.transform.parent.position;

                Vector2 goalPosition = new Vector2(hiParentPosition.x + (hi.facingRight?1:-1), hiParentPosition.y);
                Vector2 between = new Vector2(goalPosition.x - transform.position.x, goalPosition.y - transform.position.y);
                rb.AddForce(((between + (hiParentVelocity / 10)) * hi.knockback));
            }
            else
            {
                rb.velocity = Vector2.zero;
                imui.ChangeHealth(-hi.damage);
                Vector2 angleOfForce = new Vector2(Mathf.Cos(Mathf.Deg2Rad * hi.angle), Mathf.Sin(Mathf.Deg2Rad * hi.angle));

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
        Debug.Log("collision exit");
        if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            clm.RemoveLocker(wallcling);
            collidedWallSide = 0;
            touchingWall = false;
            wallTouching = null;
        }

        if (currentOverlaps.Contains(collision.collider)) { currentOverlaps.Remove(collision.collider); };
    }
}
