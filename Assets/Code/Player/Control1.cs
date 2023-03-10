using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Control1 : MonoBehaviour
{
    //February 14, 2023, 10:08PM


    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;
    [SerializeField] float wallJumpVerticalOoOne = 0.5f;
    public bool ignoreGravity = false;
    public bool intangible = false;

    //speeds
    [SerializeField] float airSpeed = 10;
    [SerializeField] float airAccel = 10;
    [SerializeField] float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    [SerializeField] float initialJumpForce = 10;

    //windows
    [SerializeField] float shorthopWindow = 3;
    [SerializeField] float walljumpShorthopWindow = 3;

    //resources
    [SerializeField] float maxHealth = 100;
    [SerializeField] float currentHealth;
    [SerializeField] float maxDashCharge = 100;
    [SerializeField] float dashCharge;
    [SerializeField] float dashForce = 10;

    //components
    Rigidbody2D rb;
    Collider2D bc;
    Animator anim;
    SpriteRenderer sr;
    ControlLockManager clm;
    PlayerInputManager pim;
    ActivateHitbox ah;
    HitboxInteractionManager him;
    Slider healthBar;
    ParticleSystem trailps;
    [SerializeField] AudioManager am;

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

    Collider2D wallTouching;
    bool touchingWall = false;
    public float minimumSpeedForHitstun = 50;
    public int framesInHitstun = 0;
    public bool facingRight = true;

    //Wall Collision
    int collidedWallSide;
    float collidedWallSlope;
    [SerializeField] GameObject platformCollider;

    //objects
    public GameObject ball;

    //frame counter
    public int frame = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        pim = GetComponent<PlayerInputManager>();
        clm = GetComponent<ControlLockManager>();
        ah = GetComponent<ActivateHitbox>();
        healthBar = ((Canvas)GameObject.FindAnyObjectByType(typeof(Canvas))).transform.GetChild(1).GetComponent<Slider>();
        trailps = GetComponent<ParticleSystem>();

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        rb.sharedMaterial = notBouncy;

        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        dashCharge = maxDashCharge;
        //_________

        friction /= 100; //adjusting friction to make it smaller because friction's effect is massive
    }

    public void VerticalResponse(CharacterInput input)
    {
        if (input.Direction.current.y < -0.5)
        {
            platformCollider.SetActive(false);
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
        if (dashCharge >= maxDashCharge)
        {
            clm.AddLocker(inAnim);
            anim.SetBool("Dash", true);

            rb.velocity = Vector2.zero;
        }
        else
        {
            // sound effect for no dash charge goes here
        }
    }

    public void Dash()
    {
        rb.AddForce(pim.GetCurrentDirectional().current * dashForce);
        StartStopTrail(1);
        clm.RemoveLocker(wallcling);
    }

    public void FLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            Flip(input);

            anim.SetBool("FLightAttack", true);
            clm.AddLocker(inAnim);
        }
    }

    public void UpLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            anim.SetBool("UpLightAttack", true);
            clm.AddLocker(inAnim);
        }
    }

    public void DownLightResponse(CharacterInput input)
    {
        if (input.IsHeld() || input.IsPending())
        {
            anim.SetBool("DownLightAttack", true);
            clm.AddLocker(inAnim);
        }
    }

    public void JumpResponse(CharacterInput input)
    {
        pim.CacheInput(input);

        if (clm.activeLockers.Contains(grounded))
        {
            clm.AddLocker(inAnim);
            anim.SetBool("Jumpsquat", true);
        }
        else if (clm.activeLockers.Contains(wallcling))
        {
            clm.AddLocker(inAnim);
            anim.SetBool("WallJumpSquat", true);
        }
    }

    public void ReduceVelocityByFactor(int factor)
    {
        rb.velocity /= factor;
    }

    public void SlowSpeed(float magnitude)
    {
        float relevantSpeed = clm.activeLockers.Contains(grounded) ? groundSpeed : airSpeed / 5;

        if (Mathf.Abs(rb.velocity.x) > relevantSpeed)
        {

            rb.AddForce(new Vector2 (rb.velocity.x / -magnitude, 0));
        }
        if (Mathf.Abs(rb.velocity.y) > relevantSpeed)
        {
            rb.AddForce(new Vector2 (0, rb.velocity.y / -magnitude));
        }
    }

    public void SwitchIfAttacking(string newAnimBool)
    {
        if (pim.BufferInputExists(ControlLock.Controls.ATTACK))
        {
            anim.SetBool(newAnimBool, true);
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

    public void ApplyWallJumpForce() // called by walljump animation
    {
        rb.velocity = Vector2.zero;

        CharacterInput initialJumpInput = pim.GetCachedInput(ControlLock.Controls.JUMP);
        if ((pim.GetCachedInput(ControlLock.Controls.JUMP).Duration >= walljumpShorthopWindow) && initialJumpInput.IsHeld())
        {
            rb.AddForce(1.8f * initialJumpForce * new Vector2(-0.75f * collidedWallSide, wallJumpVerticalOoOne));
        }
        else
        {
            rb.AddForce(1.4f * initialJumpForce * new Vector2(-0.75f * collidedWallSide, wallJumpVerticalOoOne));
        }
        clm.RemoveLocker(wallcling);
        collidedWallSide = 0;
        touchingWall = false;
        wallTouching = null;

        // it is ridiculous how many things I have to set here, something about wall mechanics should probably be reworked
    }

    public void ApplyJumpForce() // called by jump animation
    {
        CharacterInput initialJumpInput = pim.GetCachedInput(ControlLock.Controls.JUMP);

        if ((initialJumpInput.Duration >= shorthopWindow) && initialJumpInput.IsHeld())
        {
            rb.AddForce(1.5f * initialJumpForce * Vector2.up);
        }
        else
        {
            rb.AddForce(initialJumpForce * Vector2.up);
        }
        clm.RemoveLocker(grounded);
    }

    public void StartStopTrail(int startstop)
    {
        if (startstop == 1) { trailps.Play(); }
        else { trailps.Stop(); }

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

        if (healthBar.value != currentHealth)
        {
            healthBar.value -= (healthBar.value - currentHealth)/10;

            if (healthBar.value - currentHealth < 0.5f)
            {
                healthBar.value = currentHealth;
            }
        }
        
        ManageForces();

        void ManageForces()
        {
            if (clm.activeLockers.Contains(grounded))
            {
                if (Mathf.Round(pim.GetCurrentDirectional().current.x) != Mathf.Sign(rb.velocity.x) ||  !clm.ControlsAllowed(ControlLock.Controls.HORIZONTAL)
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
    }

    public void PlaySoundFromAnimator(string name)
    {
        am.PlaySound(name);
    }

    public void Hit(Collider2D collider, bool enterOrExitHitstun = true) // add to calculation for direction
    {
        if (enterOrExitHitstun)
        {
            clm.AddLocker(hitstun);
            rb.sharedMaterial = bouncy;
            HitboxInfo hi = collider.gameObject.GetComponent<HitboxInfo>();

            currentHealth -= hi.damage;

            Vector2 angleOfForce;
            angleOfForce = new Vector2(Mathf.Cos(Mathf.Rad2Deg * hi.angle), Mathf.Sin(Mathf.Rad2Deg * hi.angle));
            if (hi.facingRight)
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

            Debug.Log(angleOfForce);
            rb.AddForce(angleOfForce * hi.knockback);
        }
        else
        {
            clm.RemoveLocker(hitstun);
            rb.sharedMaterial = notBouncy;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Standable")
        {
            if (rb.velocity.y <= 0) // this is 100% a problem. idk how but def a problem
            {
                clm.AddLocker(grounded);
                anim.SetBool("Grounded", true);
            }
        }
        else if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            collidedWallSide = (int)Mathf.Sign(collision.GetContact(0).point.x - transform.position.x);
            touchingWall = true;
            wallTouching = collision.collider;
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
        if (collision.gameObject.tag == "Standable")
        {
            clm.RemoveLocker(grounded);
            anim.SetBool("Grounded", false);

        }
        else if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            clm.RemoveLocker(wallcling);
            collidedWallSide = 0;
            touchingWall = false;
            wallTouching = null;
        }
    }
}
