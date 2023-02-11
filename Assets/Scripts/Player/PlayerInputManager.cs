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

    [SerializeField] Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>> inputBuffers = new Dictionary<ControlLock.Controls, Pair<int, InputAction.CallbackContext>>();
    [SerializeField] Dictionary<ControlLock.Controls, Action<InputAction.CallbackContext>> inputEvents = new Dictionary<ControlLock.Controls, Action<InputAction.CallbackContext>>();
    [SerializeField] ControlLock.Controls[] controlPriorityList = { 
        ControlLock.Controls.JUMP, 
        ControlLock.Controls.ATTACK, 
        ControlLock.Controls.SPECIAL, 
        ControlLock.Controls.VERTICAL, 
        ControlLock.Controls.HORIZONTAL
    };
    [SerializeField] int inputBufferDuration = 10;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        inputEvents[ControlLock.Controls.HORIZONTAL] = InputHorizontal;
        inputEvents[ControlLock.Controls.VERTICAL] = InputVertical;
        inputEvents[ControlLock.Controls.JUMP] = InputJump;
        inputEvents[ControlLock.Controls.ATTACK] = InputAttack;
        inputEvents[ControlLock.Controls.SPECIAL] = InputSpecial;
    }

    private void FixedUpdate()
    {
        foreach (ControlLock.Controls control in controlPriorityList)
        {
            if (inputBuffers.TryGetValue(control, out Pair<int, InputAction.CallbackContext> val) && val.left > 0)
            {
                if (CanInput(control))
                {
                    val.left = 0;
                    inputEvents[control].Invoke(val.right);
                }
                else if (val.right.canceled)
                {
                    val.left--;
                }
            }
        }
    }

    public void OnHorizontal(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.HORIZONTAL;
        inputBuffers[control] = new Pair<int, InputAction.CallbackContext>(inputBufferDuration, ctxt);
    }

    public void InputHorizontal(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {
            playerController.SetHorizontal((float)(ctxt.ReadValue<float>()));
        }
        else if (ctxt.canceled)
        {
            playerController.SetHorizontal(0);
        }
    }

    public void OnVertical(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.VERTICAL;
        inputBuffers[control] = new Pair<int, InputAction.CallbackContext>(inputBufferDuration, ctxt);
    }

    public void InputVertical(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {

        }
        else if (ctxt.canceled)
        {

        }
    }

    public void OnJump(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.JUMP;
        inputBuffers[control] = new Pair<int, InputAction.CallbackContext>(inputBufferDuration, ctxt);
    }

    public void InputJump(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {

        }
        else if (ctxt.canceled)
        {

        }
    }

    public void OnAttack(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.ATTACK;
        inputBuffers[control] = new Pair<int, InputAction.CallbackContext>(inputBufferDuration, ctxt);
    }

    public void InputAttack(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {

        }
        else if (ctxt.canceled)
        {

        }
    }

    public void OnSpecial(InputAction.CallbackContext ctxt)
    {
        ControlLock.Controls control = ControlLock.Controls.SPECIAL;
        inputBuffers[control] = new Pair<int, InputAction.CallbackContext>(inputBufferDuration, ctxt);
    }

    public void InputSpecial(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {

        }
        else if (ctxt.canceled)
        {

        }
    }

    private bool CanInput(ControlLock.Controls controls)
    {
        bool controlsAllowed = controlLockManager.ControlsAllowed(controls);
        if (debugMessages)
        {
            string output = "Control \"" + controls.ToString() + (controlsAllowed ? "\" allowed." : "\" not allowed.");
            Debug.Log(output);
        }
        return controlsAllowed;
    }
}
