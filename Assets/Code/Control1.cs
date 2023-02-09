using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control1 : MonoBehaviour
{
    //physics
    //all speeds and accels are used as multipliers when adding or subtracting speed
    [SerializeField] float weight = 10;
    [SerializeField] float fallSpeed = 10;
    [SerializeField] float fallAccel = 10;
    [SerializeField] float friction = 10;

    //speeds
    [SerializeField] float airSpeed = 10;
    [SerializeField] float airAccel = 10;
    [SerializeField] float groundSpeed = 10;
    [SerializeField] float groundAccel = 10;
    [SerializeField] float jumpHeight = 10;
    [SerializeField] float minJumpHeight = 10;
    [SerializeField] float jumpSpeed = 10;
    [SerializeField] float initialJumpForce = 10;

    //delays
    [SerializeField] float jumpSquat = 3;

    //components
    Rigidbody2D rb;
    Collider2D bc;

    //input
    Vector2 dirInput = new Vector2(0, 0); //contains the directional inputs (x,y) -1 to 1 on last update frame
    float spaceBuffer = 0;

    //buffer
    public float bufferLength = 5;

    //conditions
    bool grounded = true;
    bool inHitstun;
    bool wallcling;
    bool inAnimation;
    bool canMoveDir = true;
    bool canButtons = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();

        friction /= 100;
    }

    void Update() //records inputs every update frame, and changes buffer to reflect new input
    {
        if (spaceBuffer > 0) // this should be reworked into a function that controls all buffering
        {
            spaceBuffer -= Time.deltaTime;
        }

        dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // if an input is recorded, reset the buffer for that input
        if (Input.GetKey(KeyCode.Space))
        {
            spaceBuffer = bufferLength;
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
                    rb.velocity = new Vector2(0, rb.velocity.y);
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
                    StartCoroutine("Jump");
                }
            }
        }
    }

    int CheckBuffer(int input)
    {
        if (input > 0)
        {
            input -= 1;
            return input;
        }
        else
        {
            return input;
        }
    }

    IEnumerator Jump()
    {
        canButtons = false;
        canMoveDir = false;
        float timer = 0;
        while (timer < jumpSquat) // waits for jumpsquat timer
        {
            timer += Time.deltaTime;
            yield return null;
        }

        float jumpTime = 0; // how long as jump been active
        rb.AddForce(new Vector2(0, initialJumpForce)); // adds initial jump force
        grounded = false;
        canButtons = true;
        canMoveDir = true;

        for (int i = 0; i < jumpHeight; i++) //applies jumpSpeed boost until: jump height is maxed out || space is released
        {
            rb.AddForce(new Vector2(0, jumpSpeed));
            if (spaceBuffer == 0 && jumpTime > minJumpHeight)
            {
                break;
            }
            jumpTime += Time.deltaTime; ;
            yield return null;
        }

        Debug.Log("jump end");
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
