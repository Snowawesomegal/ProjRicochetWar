using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerUIBrowser : MonoBehaviour
{
    public int currentButton = -1;

    public event Action<Vector2> NavigateBehavior;
    public event Action SubmitBehavior;
    public event Action BackBehavior;

    public void OnNavigate(InputAction.CallbackContext ctxt)
    {
        Vector2 vec = ctxt.ReadValue<Vector2>();
        Debug.Log("Navigating: " + vec);
        NavigateBehavior?.Invoke(vec);
    }

    public void OnSubmit(InputAction.CallbackContext ctxt)
    {
        Debug.Log("Submitting");
        SubmitBehavior?.Invoke();
    }

    public void OnBack(InputAction.CallbackContext ctxt)
    {
        Debug.Log("Hit back button");
        BackBehavior?.Invoke();
    }
}
