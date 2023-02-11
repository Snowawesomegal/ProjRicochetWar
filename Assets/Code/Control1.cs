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
    [SerializeField] float maxJumpFrames = 10;
    [SerializeField] float minJumpFrames = 10;
    [SerializeField] float jumpSpeed = 10;
    [SerializeField] float initialJumpForce = 10;

    //delays
    [SerializeField] float jumpSquat = 3;
    [SerializeField] float wallJumpSquat = 3;

    //components
    Rigidbody2D rb;
    Collider2D bc;

    //input
    Vector2 dirInput = new Vector2(0, 0); //contains the directional inputs (x,y) -1 to 1 on last update frame

    //buffer
    public int bufferLength = 5; //how long in seconds an input that is not currently valid will wait to be valid

    //states
    List<State> activeStates = new List<State>();
    State grounded = new State();
    State touchingWall = new State();
    State inHitstun = new State(false, false, false, false, false, false);
    State inAnim = new State(false, false, false, false, false, false);
    State wallcling = new State();
    Collider2D wallTouching;
    bool jumping = false;
    bool wallJumping = false;
    int jumpFrames = 0;
    bool jumpHeldContinuously;

    //buttons
    List<Button> allButtons = new List<Button>();
    Button space;

    //Wall Collision
    int collidedWallSide;
    float collidedWallSlope;

    public class Button
    {
        public Button(KeyCode keyCode2, int newDefaultBuffer)
        {
            defaultBuffer = newDefaultBuffer;
            keyCode = keyCode2;
        }
        public int defaultBuffer = 5;
        public bool pressed = false;
        public int buffer = 0;
        public int consecutive = 0;
        public KeyCode keyCode;
    }

    public class State
    {
        public State
            (
            bool jump2 = true, bool dir2 = true, bool light2 = true, bool heavy2 = true,
            bool special2 = true, bool wallCling2 = true, bool dash2 = true
            )
        {
            jump = jump2;
            dir = dir2;
            light = light2;
            heavy = heavy2;
            special = special2;
            wallCling = wallCling2;
            dash = dash2;
        }

        public bool jump = true;
        public bool dir = true;
        public bool light = true;
        public bool heavy = true;
        public bool special = true;
        public bool wallCling = true;
        public bool dash = true;
    }



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<Collider2D>();

        friction /= 100; //adjusting friction to make it smaller because friction's effect is massive

        space = new Button(KeyCode.Space, bufferLength);

        allButtons.Add(space);
    }

    void Update() //records inputs every update frame, and changes buffer to reflect new input
    {
        dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        foreach (Button i in allButtons)
        {
            if (Input.GetKeyDown(i.keyCode))
            {
                i.buffer = i.defaultBuffer;
                Debug.Log(i.keyCode + " buffer set");
            }
            if (Input.GetKey(i.keyCode))
            {
                i.pressed = true;
            }
            else
            {
                i.pressed = false;
            }
        }
    }

    private void FixedUpdate()
    {

        if (CheckDir())
        {
            DirectionalResponse();
        }
        if (CheckJump())
        {
            JumpResponse();
        }
        if (jumping)
        {
            Jump();
        }
        if (wallJumping)
        {
            WallJump();
        }

        ManageForces();

        void ManageForces()
        {
            if (activeStates.Contains(grounded))
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
                if (!activeStates.Contains(wallcling))
                {
                    rb.AddForce(Vector2.down * fallAccel);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -fallSpeed, 999999));
                    //this is a placeholder, see directional response's explanation below
                }

            }
        }

        void DirectionalResponse()
        {
            if (!activeStates.Contains(wallcling))
            {
                if (activeStates.Contains(grounded))
                {
                    rb.AddForce(dirInput * groundAccel);
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -groundSpeed, groundSpeed), rb.velocity.y);
                    //above line caps horizontal speed at groundspeed every frame
                    //this is a placeholder- placing a hard cap on horizontal speed fundamentally restricts future movement and must be changed
                    //best solution is to constantly reduce speed until speed is within desired parameters while touching ground and not in hitstun

                }
                else
                {
                    if (activeStates.Contains(touchingWall))
                    {
                        if (collidedWallSide == dirInput.x)
                        {
                            activeStates.Add(wallcling);
                            rb.velocity = Vector2.zero;
                        }
                    }

                    rb.AddForce(dirInput * airAccel);
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -airSpeed, airSpeed), rb.velocity.y);
                    //same as grounded
                }
            }
            else
            {
                if (dirInput.x != collidedWallSide)
                {
                    activeStates.Remove(wallcling);
                }
            }

        }

        void JumpResponse()
        {
            if (activeStates.Contains(grounded))
            {
                if (space.buffer > 0)
                {
                    activeStates.Add(inAnim);
                    space.buffer = 0;
                    jumping = true;
                    jumpFrames = 1;
                }
            }
            else if (activeStates.Contains(wallcling))
            {
                if (space.buffer > 0)
                {
                    activeStates.Add(inAnim);
                    space.buffer = 0;
                    wallJumping = true;
                    jumpFrames = 1;
                }
            }

        }

        void WallJump()
        {
            switch (jumpFrames)
            {
                case int n when (n < wallJumpSquat):
                    break;
                case int n when (n == wallJumpSquat):
                    if (space.consecutive <= 2)
                    {
                        rb.AddForce(new Vector2(-0.75f * collidedWallSide, 0.75f) * initialJumpForce * 1.3f);
                    }
                    else
                    {
                        rb.AddForce(new Vector2(-0.75f * collidedWallSide, 0.75f) * initialJumpForce * 2f);
                    }
                    wallJumping = false;
                    activeStates.Remove(inAnim);
                    break;
            }
            jumpFrames += 1;
        }

        void Jump() //if we were using some kind of animation cue this if statement would be that instead
        {
            switch (jumpFrames)
            {
                case int n when (n < jumpSquat):
                    break;
                case int n when (n == jumpSquat):
                    if (space.consecutive <= 3)
                    {
                        rb.AddForce(Vector2.up * initialJumpForce);
                    }
                    else
                    {
                        rb.AddForce(Vector2.up * initialJumpForce * 1.5f);
                    }
                    activeStates.Remove(inAnim);
                    jumping = false;
                    break;
            }
            jumpFrames += 1;
        }

        foreach (Button i in allButtons)
        {
            if (Input.GetKey(i.keyCode))
            {
                i.consecutive += 1;
            }
            else
            {
                i.consecutive = 0;
            }

            if (i.buffer > 0)
            {
                i.buffer -= 1;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "BottomWall" && !activeStates.Contains(inHitstun))
        {
            activeStates.Add(grounded);
        }
        else if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            collidedWallSide = (int)Mathf.Sign(collision.GetContact(0).point.x - transform.position.x);
            activeStates.Add(touchingWall);
            wallTouching = collision.collider;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "BottomWall")
        {
            activeStates.Remove(grounded);
        }
        else if (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall")
        {
            activeStates.Remove(touchingWall);
            collidedWallSide = 0;
            wallTouching = null;
        }
    }


    bool CheckDir() //these work but god they're stupid
    {
        if (activeStates.Count > 0)
        {
            foreach (State i in activeStates)
            {
                if (!i.dir)
                {
                    return false;
                }
            }
        }
        return true;
    }
    bool CheckJump()
    {
        if (activeStates.Count > 0)
        {
            foreach (State i in activeStates)
            {
                if (!i.jump)
                {
                    return false;
                }
            }
        }
        return true;
    }
    bool CheckWallCling()
    {
        if (activeStates.Count > 0)
        {
            foreach (State i in activeStates)
            {
                if (!i.wallCling)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
