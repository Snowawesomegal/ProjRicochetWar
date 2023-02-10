using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ControlLockManager), typeof(PlayerController))]
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] ControlLockManager controlLockManager;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void OnHorizontal(InputAction.CallbackContext ctxt)
    {
        if (!controlLockManager.ControlsAllowed(ControlLock.Controls.HORIZONTAL))
        {
            Debug.Log("Not allowed horizontal input!");
            return;
        } else
        {
            Debug.Log("Success inputting horizontal");
        }
        if (ctxt.started || ctxt.performed)
        {
            playerController.SetHorizontal((float)(ctxt.ReadValue<float>()));
        } else if (ctxt.canceled)
        {
            playerController.SetHorizontal(0);
        }
    }

    public void OnVertical(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started || ctxt.performed)
        {

        } else if (ctxt.canceled)
        {

        }
    }
}
