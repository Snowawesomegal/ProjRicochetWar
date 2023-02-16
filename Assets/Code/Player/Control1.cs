using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Control1 : MonoBehaviour
{
    //February 14, 2023, 10:08PM


    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;

    //speeds
    [SerializeField] float airSpeed = 10;
    [SerializeField] float airAccel = 10;
    [SerializeField] float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    [SerializeField] float initialJumpForce = 10;

    //components
    Rigidbody2D rb;
    Collider2D bc;
    Animator anim;
    SpriteRenderer sr;
    ControlLockManager clm;
    PlayerInputManager pir;

    public PhysicsMaterial2D bouncy;
    public PhysicsMaterial2D notBouncy;

    //input
    public Vector2 dirInput = new Vector2(0, 0); //contains the directional inputs (x,y) -1 to 1 on last update frame

    //buffer
    public int bufferLength = 5; //how long in seconds an input that is not currently valid will wait to be valid

    //Control Lockers
    public StandardControlLocker grounded;
    public StandardControlLocker airborne;
    public StandardControlLocker hitstun;
    public StandardControlLocker inAnim;
    public StandardControlLocker wallcling;

    Collider2D wallTouching;
    bool touchingWall = false;
    public float minimumSpeedForHitstun = 50;
    public int framesInHitstun = 0;
    public bool facingRight = true;

    //Wall Collision
    int collidedWallSide;
    float collidedWallSlope;

    //objects
    public GameObject ball;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        pir = GetComponent<PlayerInputManager>();
        clm = GetComponent<ControlLockManager>();

        rb.sharedMaterial = notBouncy;

        friction /= 100; //adjusting friction to make it smaller because friction's effect is massive
    }

    public void VerticalResponse()
    {

    }

    public void HorizontalResponse()
    {
        Debug.Log("horizontal response");
        if (clm.activeLockers.Contains(wallcling))
        {
            if (dirInput.x != collidedWallSide)
            {
                clm.RemoveLocker(wallcling);
            }
        }
        else
        {
            if (grounded)
            {
                Debug.Log("grounded and moving");
                rb.AddForce(dirInput * groundAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y);
                //above line caps horizontal speed at groundspeed every frame
                //this is a placeholder- placing a hard cap on horizontal speed fundamentally restricts future movement and must be changed
                //best solution is to constantly reduce speed until speed is within desired parameters while touching ground and not in hitstun
            }
            else
            {
                if (touchingWall)
                {
                    if (collidedWallSide == dirInput.x)
                    {
                        clm.AddLocker(wallcling);
                        rb.velocity = Vector2.zero;
                    }
                }
                rb.AddForce(dirInput * airAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -airSpeed, airSpeed), rb.velocity.y);
                //same as grounded
            }

            if (dirInput.x > 0)
            {
                facingRight = true;
                sr.flipX = false;
            }
            else if (dirInput.x < 0)
            {
                facingRight = false;
                sr.flipX = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (clm.activeLockers.Contains(hitstun))
        {
            HitstunResponse(); // if in hitstun, once per frame, check moving slow enough that hitstun is over
        }

        ManageForces();

        void ManageForces()
        {
            if (clm.activeLockers.Contains(grounded))
            {
                if ((Mathf.Sign(dirInput.x) != Mathf.Sign(rb.velocity.x)) || (dirInput.x == 0)) //if grounded + not holding the direction of motion;
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
                if (!clm.activeLockers.Contains(wallcling))
                {
                    rb.AddForce(Vector2.down * fallAccel);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -fallSpeed, 999999));
                    //this is a placeholder, see directional response's explanation below
                }

            }
        }
    }

    void HeavyResponse()
    {
        anim.SetBool("HeavyAttack", true);
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

    void JumpResponse()
    {
        if (clm.activeLockers.Contains(grounded))
        {
            clm.AddLocker(inAnim);
        }
        else if (clm.activeLockers.Contains(wallcling))
        {
            clm.AddLocker(inAnim);
        }
    }

    public void WallJump() // called by walljump animation
    {
        if (3 <= 2) //TODO change "3" to consecutive held frames of jump input
        {
            rb.AddForce(new Vector2(-0.75f * collidedWallSide, 0.75f) * initialJumpForce * 1.3f);
        }
        else
        {
            rb.AddForce(new Vector2(-0.75f * collidedWallSide, 0.75f) * initialJumpForce * 2f);
        }
        clm.RemoveLocker(inAnim);
    }

    public void Jump() // called by jump animation
    {
        if (4 <= 3) //TODO change "4" to consecutive held frames of jump input
        {
            rb.AddForce(Vector2.up * initialJumpForce);
        }
        else
        {
            rb.AddForce(Vector2.up * initialJumpForce * 1.5f);
        }
        clm.RemoveLocker(inAnim);
        clm.RemoveLocker(grounded);
    }

    void Hit(Collider2D collider, bool enterOrExitHitstun = true)
    {
        if (enterOrExitHitstun)
        {
            clm.AddLocker(hitstun);
            rb.sharedMaterial = bouncy;
            HitboxInfo hi = collider.gameObject.GetComponent<HitboxInfo>();
            Vector2 objectVelocity = hi.owner.GetComponent<Rigidbody2D>().velocity;

            Vector2 angleOfForce;
            angleOfForce = new Vector2(Mathf.Cos(Mathf.Rad2Deg * hi.angle), Mathf.Sin(Mathf.Rad2Deg * hi.angle) * Mathf.Sign(objectVelocity.x));
            if (hi.facingRight)
            {
                angleOfForce.x *= -1;
            }
            if (!hi.angleIndependentOfMovement)
            {
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
        if (collision.gameObject.name == "BottomWall")
        {
            clm.AddLocker(grounded);
            anim.SetBool("Grounded", true);
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
        Hit(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "BottomWall")
        {
            touchingWall = false;
            anim.SetBool("Grounded", false);
        }
        else if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            touchingWall = true;
            collidedWallSide = 0;
            wallTouching = null;
        }
    }
}
