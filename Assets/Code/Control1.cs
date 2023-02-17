using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control1 : MonoBehaviour
{
    //Summary
    //



    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float weight = 10;
    [SerializeField] float gravity = 10;
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;

    //speeds
    [SerializeField] float airSpeed = 10;
    [SerializeField] float airAccel = 10;
    [SerializeField] float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    [SerializeField] float maxJumpTime = 10;
    [SerializeField] float minJumpTime = 10;
    [SerializeField] float jumpSpeed = 10;
    [SerializeField] float initialJumpForce = 10;

    //delays
    [SerializeField] float jumpSquat = 3;

    //components
    Rigidbody2D rb;
    Collider2D bc;

    //input
    Vector2 dirInput = new Vector2(0, 0); //contains the directional inputs (x,y) -1 to 1 on last update frame

    //buffer
    public float bufferLength = 5; //how long in seconds an input that is not currently valid will wait to be valid
    float spaceBuffer = 0; //stores buffer for spacebar, also stores input for spacebar between Update and FixedUpdate

    //conditions
    [SerializeField] bool grounded = false;
    bool inHitstun;
    bool wallcling;
    bool inAnimation;
    bool canMoveDir = true; //can move directionally - whether to recognize or buffer directional inputs
    bool canButtons = true; //can input buttons - whether to recognize or buffer button inputs
    bool jumping = false;
    float jumpTimer = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();

        friction /= 100; //adjusting friction to make it smaller because friction's effect is massive
    }

    void Update() //records inputs every update frame, and changes buffer to reflect new input
    {
        if (spaceBuffer > 0) // this should be reworked into a function that controls all buffering
        {
            spaceBuffer -= Time.deltaTime;
        }

        dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // if an input is recorded, reset the buffer for that input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceBuffer = bufferLength;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if (spaceBuffer < 0.1f)
            {
                spaceBuffer = 0.1f;
            }
        }

        if (grounded)
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
    }

    private void FixedUpdate()
    {
        if (canMoveDir)
        {
            DirectionalResponse();
        }
        if (canButtons)
        {
            ButtonResponse();
        }
        if (jumping)
        {
            Jump();
        }

        ManageGravity();

        void ManageGravity()
        {
            if (!grounded)
            {
                rb.AddForce(Vector2.down * fallAccel);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -fallSpeed, 999999));
                //this is a placeholder, see directional response's explanation below
            }
        }

        void DirectionalResponse()
        {
            if (grounded)
            {
                rb.AddForce(dirInput * groundAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y);
                //above line caps horizontal speed at groundspeed every frame
                //this is a placeholder- placing a hard cap on horizontal speed fundamentally restricts future movement and must be changed
                //best solution is to constantly reduce speed until speed is within desired parameters while touching ground and not in hitstun

            }
            else
            {
                rb.AddForce(dirInput * airAccel);
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -airSpeed, airSpeed), rb.velocity.y);
                //same as grounded
            }
        }

        void ButtonResponse()
        {
            if (grounded)
            {
                if (spaceBuffer > 0)
                {
                    Debug.Log("Jump");
                    spaceBuffer = 0;
                    jumping = true;
                    StartCoroutine("Jump");
                }
            }
        }

        void JumpRework() //in progress rework of jump mechanic, feel free to delete or change this, it is not functional.
        {
            if (jumpTimer == 0)
            {
                canButtons = false; //disables control during jumpsquat
                canMoveDir = false;
            }
            else if (jumpTimer > jumpSquat)
            {
                float timer = 0;
                if (timer < jumpSquat) // waits for jumpsquat timer
                {
                    timer += Time.deltaTime;
                }
                rb.AddForce(new Vector2(0, initialJumpForce)); // adds initial jump force
                grounded = false;
                canButtons = true; //enables control after jumpsquat. If canceled before this, but after disabled, things could get fucked
                canMoveDir = true;
            }

            jumpTimer += Time.deltaTime;
        }
    }

    IEnumerator Jump() //Needs rework - applies force during update frames, making jump speeds inconsistent depending on framerate //TODO
    {
        canButtons = false; //disables control during jumpsquat
        canMoveDir = false;
        float timer = 0;
        while (timer < jumpSquat) // waits for jumpsquat timer
        {
            timer += Time.deltaTime;
            yield return null;
        }
        rb.AddForce(new Vector2(0, initialJumpForce)); // adds initial jump force
        grounded = false;
        canButtons = true; //enables control after jumpsquat. If canceled before this, but after disabled, things could get fucked
        canMoveDir = true;

        float jumpTime = 0; // how long as jump been active
        while (jumpTime < maxJumpTime) //applies jumpSpeed boost until jump height is: (maxed out || space is released) && > mintime
        {
            rb.AddForce(new Vector2(0, jumpSpeed));
            if (spaceBuffer <= 0 && jumpTime > minJumpTime)
            {
                break;
            }
            jumpTime += Time.deltaTime;
            yield return null;
        }
        spaceBuffer = 0;
        yield break;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "BottomWall" && !inHitstun)
        {
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "BottomWall")
        {
            grounded = false;
            Debug.Log("airborne");
        }
    }
}