using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ControlLockManager), typeof(PlayerController))]
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] ControlLockManager controlLockManager;
    [SerializeField] bool debugMessages = false;

    [SerializeField] Control1 c1;

    //[SerializeField] Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>> inputBuffers = new Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>>();
    [SerializeField] Dictionary<ControlLock.Controls, Action<CharacterInput>> inputEvents = new Dictionary<ControlLock.Controls, Action<CharacterInput>>();
    ControlLock.Controls[] controlPriorityList = { 
        ControlLock.Controls.JUMP,
        ControlLock.Controls.DASH,
        ControlLock.Controls.ATTACK, 
        ControlLock.Controls.SPECIAL, 
        ControlLock.DIRECTIONAL_CONTROLS
    };
    [SerializeField] int inputBufferDuration = 10;

    [SerializeField] bool inputBufferDebugMessages = false;
    PlayerInputBuffer pib = new PlayerInputBuffer();

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
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
    }

    void FixedUpdate()
    {
        pib.debugMessages = inputBufferDebugMessages;
        foreach (ControlLock.Controls control in controlPriorityList)
        {
            if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
            {
                bool canInput = CanInput(control, out string debugStr);
                pib.MaintainBuffer(buffer, control, canInput);
                if (canInput && pib.GrabImmediateInput(buffer, out CharacterInput input))
                {
                    inputEvents[control].Invoke(input);
                    DebugAllowedInput(debugStr, input);
                }
                else
                    DebugAllowedInput(debugStr, null);
            }
        }
    }

    private void AcceptBufferInput(ControlLock.Controls control, InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.canceled) //ctxt.performed || 
        {
            pib.AcceptInput(control, inputBufferDuration, ctxt);
            if (pib.TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer))
            {
                pib.CleanBuffer(buffer, control);
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
        c1.HorizontalResponse(input);
        c1.VerticalResponse(input);
    }

    public void OnDash(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.DASH;
        AcceptBufferInput(control, ctxt);
    }
    public void InputDash(CharacterInput input)
    {
        // I don't actually have any of the dash mechanics done, so nothing is here
    }

    public void OnJump(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.JUMP;
        AcceptBufferInput(control, ctxt);
    }

    public void InputJump(CharacterInput input)
    {
        c1.JumpResponse(input);
    }

    public void OnAttack(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.ATTACK;
        AcceptBufferInput(control, ctxt);
    }

    public void InputAttack(CharacterInput input)
    {
        c1.FTilt();

        // TODO: logic to implement directional attack call
        // basically if elses testing the input.Direction value
    }

    public void OnSpecial(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.SPECIAL;
        AcceptBufferInput(control, ctxt);
    }

    public void InputSpecial(CharacterInput input)
    {
        // TODO: logic to implement directional attack call
        // basically if elses testing the input.Direction value
    }

    private bool CanInput(ControlLock.Controls controls, out string debugStr)
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
