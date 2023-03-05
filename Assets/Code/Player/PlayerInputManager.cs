using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ControlLockManager), typeof(Control1))]
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] Control1 playerController;
    [SerializeField] ControlLockManager controlLockManager;
    [SerializeField] bool debugMessages = false;

    //[SerializeField] Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>> inputBuffers = new Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>>();
    [SerializeField] Dictionary<ControlLock.Controls, Action<CharacterInput>> inputEvents = new Dictionary<ControlLock.Controls, Action<CharacterInput>>();
    ControlLock.Controls[] controlPriorityList = { 
        ControlLock.Controls.JUMP,
        ControlLock.Controls.DASH,
        ControlLock.Controls.MOVEMENT,
        ControlLock.Controls.ATTACK, 
        ControlLock.Controls.HEAVY,
        ControlLock.Controls.SPECIAL, 
        ControlLock.DIRECTIONAL_CONTROLS
    };
    [SerializeField] int inputBufferDuration = 10;

    [SerializeField] bool inputBufferDebugMessages = false;
    PlayerInputBuffer pib = new PlayerInputBuffer();

    [SerializeField] public float playerVerticalAttackThreshold = 70f;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<Control1>();
        }
        if (controlLockManager == null)
        {
            controlLockManager = GetComponent<ControlLockManager>();
        }

        pib.InitializeBuffers(controlPriorityList);

        inputEvents[ControlLock.Controls.JUMP] = InputJump;
        inputEvents[ControlLock.Controls.ATTACK] = InputAttack;
        inputEvents[ControlLock.Controls.SPECIAL] = InputSpecial;
        inputEvents[ControlLock.Controls.DASH] = InputDash;
        inputEvents[ControlLock.DIRECTIONAL_CONTROLS] = InputDirectional;
        inputEvents[ControlLock.Controls.HEAVY] = InputHeavy;
        inputEvents[ControlLock.Controls.MOVEMENT] = InputMovement;
    }

    void FixedUpdate()
    {
        // update debug messages variable
        pib.debugMessages = inputBufferDebugMessages;
        // cache the current directional input
        pib.CacheCurrentDirectional();

        //Debug.Log("Current directional: " + pib.CachedDirectional.current);

        // iterate over each control in the priority list (in the priority order)
        foreach (ControlLock.Controls control in controlPriorityList)
        {
            // try to get the buffer for each control (and check if buffer has any inputs)
            if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
            {
                // check if the input is locked
                bool canInput = CanInput(control, out string debugStr);
                // run the maintenance method for the buffer (merging sequential inputs, removing expired and interrupted inputs)
                pib.MaintainBuffer(buffer, control, true);
                // attempt to grab the earliest buffered input
                if (pib.PeekNextInput(buffer, out CharacterInput input))
                {
                    // if the input is allowed and not interrupted
                    if (canInput && !input.IsInterrupted() && pib.TryPopNextInput(buffer, out input))
                    {
                        // invoke the input event
                        inputEvents[control].Invoke(input);
                        // assign the input process stage to be processing
                        input.ProcessingStage = CharacterInput.InputProcessStage.PROCESSING;
                        DebugAllowedInput(debugStr + ", marking input as processed, " + input.ToString(), input);
                    } else
                    {
                        // if it was processing before (and isn't directional, bc directionals can't be interrupted)
                        if (input.IsProcessing() && input.CacheControl != ControlLock.DIRECTIONAL_CONTROLS)
                        {
                            // update the processing stage to be interrupted
                            input.ProcessingStage = CharacterInput.InputProcessStage.INTERRUPTED;
                        }
                        DebugAllowedInput(debugStr + ", " + input.ToString(), input);
                    }
                }
                else
                    DebugAllowedInput(debugStr, null);
            }
        }
    }

    private void AcceptBufferInput(ControlLock.Controls control, InputAction.CallbackContext ctxt)
    {
        // accept only start and canceled inputs (press and release)
        if (ctxt.started || ctxt.canceled || ctxt.performed)
        {
            pib.AcceptInput(control, inputBufferDuration, ctxt);
            if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer))
            {
                pib.CleanBuffer(buffer, control); // attempt to merge continuous inputs
            }
        }
    }

    public void OnDirectional(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.DIRECTIONAL_CONTROLS;
        AcceptBufferInput(control, ctxt);
    }
    public void InputDirectional(CharacterInput input)
    {
        playerController.HorizontalResponse(input);
        playerController.VerticalResponse(input);
    }

    public void OnDash(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.DASH;
        AcceptBufferInput(control, ctxt);
    }
    public void InputDash(CharacterInput input)
    {
        playerController.DashResponse(input);
    }

    public void OnJump(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.JUMP;
        AcceptBufferInput(control, ctxt);
    }

    public void InputJump(CharacterInput input)
    {
        playerController.JumpResponse(input);
    }

    public void OnAttack(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.ATTACK;
        AcceptBufferInput(control, ctxt);
    }

    public void InputAttack(CharacterInput input)
    {
        CharacterInput.CardinalDirection snappedDirection = input.Direction.GetSnappedStartingDirection(playerVerticalAttackThreshold);

        switch (snappedDirection)
        {
            case CharacterInput.CardinalDirection.UP:
                if (debugMessages)
                    Debug.Log("Attacking upwards");
                playerController.UpLightResponse(input);
                break;
            case CharacterInput.CardinalDirection.DOWN:
                if (debugMessages)
                    Debug.Log("Attacking down");
                // call down attack in player controller
                break;
            case CharacterInput.CardinalDirection.LEFT:
                playerController.FLightResponse(input);
                break;
            case CharacterInput.CardinalDirection.RIGHT:
                playerController.FLightResponse(input);
                break;
            case CharacterInput.CardinalDirection.NONE:
                if (debugMessages)
                    Debug.Log("Attacking sideways " + input.Phase);
                break;
            default:
                Debug.LogError("Error - cannot determine snapped cardinal direction from player input: " + input.ToString());
                break;
        }
    }

    public void OnSpecial(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.SPECIAL;
        AcceptBufferInput(control, ctxt);
    }

    public void InputSpecial(CharacterInput input)
    {
        CharacterInput.CardinalDirection snappedDirection = input.Direction.GetSnappedStartingDirection(playerVerticalAttackThreshold);
        switch (snappedDirection)
        {
            case CharacterInput.CardinalDirection.UP:
                // call up special in player controller
                break;
            case CharacterInput.CardinalDirection.DOWN:
                // call down special in player controller
                break;
            case CharacterInput.CardinalDirection.LEFT:
            case CharacterInput.CardinalDirection.RIGHT:
            case CharacterInput.CardinalDirection.NONE:
                // call forward/side special in player controller
                break;
            default:
                Debug.LogError("Error - cannot determine snapped cardinal direction from player input: " + input.ToString());
                break;
        }
    }

    public void OnHeavy(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.HEAVY;
        AcceptBufferInput(control, ctxt);
    }
    public void InputHeavy(CharacterInput input)
    {
        CharacterInput.CardinalDirection snappedDirection = input.Direction.GetSnappedStartingDirection(playerVerticalAttackThreshold);
        switch (snappedDirection)
        {
            case CharacterInput.CardinalDirection.UP:
                playerController.UpHeavyResponse(input);
                break;
            case CharacterInput.CardinalDirection.DOWN:
                // call down heavy in player controller
                break;
            case CharacterInput.CardinalDirection.LEFT:
            case CharacterInput.CardinalDirection.RIGHT:
            case CharacterInput.CardinalDirection.NONE:
                playerController.FHeavyResponse(input);
                break;
            default:
                Debug.LogError("Error - cannot determine snapped cardinal direction from player input: " + input.ToString());
                break;
        }
    }

    public void OnMovement(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.MOVEMENT;
        AcceptBufferInput(control, ctxt);
    }
    public void InputMovement(CharacterInput input)
    {
        CharacterInput.CardinalDirection snappedDirection = input.Direction.GetSnappedStartingDirection(playerVerticalAttackThreshold);
        switch (snappedDirection)
        {
            case CharacterInput.CardinalDirection.UP:
                // call up movement in player controller
                break;
            case CharacterInput.CardinalDirection.DOWN:
                // call down movement in player controller
                break;
            case CharacterInput.CardinalDirection.LEFT:
            case CharacterInput.CardinalDirection.RIGHT:
            case CharacterInput.CardinalDirection.NONE:
                // call forward/side movement in player controller
                break;
            default:
                Debug.LogError("Error - cannot determine snapped cardinal direction from player input: " + input.ToString());
                break;
        }
    }

    public bool CanInput(ControlLock.Controls controls, out string debugStr)
    {
        bool controlsAllowed = controlLockManager.ControlsAllowed(controls);
        if (debugMessages)
        {
            string output = "Control \"" + controls.ToString() + (controlsAllowed ? "\" allowed." : "\" not allowed.");
            debugStr = output;
        }
        else
        {
            debugStr = "";
        }
        return controlsAllowed;
    }

    public CharacterInput.DirectedInput GetCurrentDirectional()
    {
        return pib.CachedDirectional;
    }

    public Vector2 GetCurrentDirection()
    {
        return pib.CachedDirectional.current;
    }

    public bool CacheInput(CharacterInput input)
    {
        pib.CacheInput(input);
        return true;
    }

    public CharacterInput GetCachedInput(ControlLock.Controls control)
    {
        if (CharacterInput.IsDirectionalControl(control))
            control = ControlLock.DIRECTIONAL_CONTROLS;
        return pib.GetCachedInput(control);
    }

    public bool TryGetCachedInput(ControlLock.Controls control, out CharacterInput input)
    {
        if (CharacterInput.IsDirectionalControl(control))
            control = ControlLock.DIRECTIONAL_CONTROLS;
        return pib.TryGetCachedInput(control, out input);
    }

    public bool BufferInputExists(ControlLock.Controls control)
    {
        if (CharacterInput.IsDirectionalControl(control))
            control = ControlLock.DIRECTIONAL_CONTROLS;
        if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
        {
            return true;
        }
        return false;
    }
    public bool BufferInputExists(ControlLock.Controls control, out CharacterInput input)
    {
        if (CharacterInput.IsDirectionalControl(control))
            control = ControlLock.DIRECTIONAL_CONTROLS;
        if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
        {
            input = buffer[0].right;
            return true;
        }
        input = null;
        return false;
    }

    private void DebugAllowedInput(string debugStart, CharacterInput input)
    {
        if (!debugMessages)
            return;
        if (input == null)
            Debug.Log(debugStart);
        else
            Debug.Log(debugStart + " Input phase: " + input.Phase.ToString());
    }
}
