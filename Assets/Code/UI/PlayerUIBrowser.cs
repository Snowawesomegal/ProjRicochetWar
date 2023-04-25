using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerUIBrowser : MonoBehaviour
{
    public int currentButton = -1;
    public int playerIndex = -1;

    public event Action<Vector2> NavigateBehavior;
    public event Action SubmitBehavior;
    public event Action BackBehavior;
    public event Action ReadyBehavior;

    [SerializeField] public float moveSensitivty = 0.1f;
    public Vector2 previousMove = Vector2.zero;

    [SerializeField] public bool debugMessages = false;

    public void OnNavigate(InputAction.CallbackContext ctxt)
    {
        Vector2 vec = ctxt.ReadValue<Vector2>();
        if (Mathf.Abs(vec.x) < moveSensitivty)
            vec.x = 0;
        if (Math.Abs(vec.y) < moveSensitivty)
            vec.y = 0;
        if (!Utility.EquivalentSigns(vec.x, previousMove.x) || !Utility.EquivalentSigns(vec.y, previousMove.y))
        {
            if (debugMessages)
                Debug.Log("Player " + playerIndex + " Navigating: " + vec);
            previousMove = vec;
            NavigateBehavior?.Invoke(vec);
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctxt)
    {
        if (debugMessages)
            Debug.Log("Player " + playerIndex + " Submitting");
        SubmitBehavior?.Invoke();
    }

    public void OnBack(InputAction.CallbackContext ctxt)
    {
        if (debugMessages)
            Debug.Log("Player " + playerIndex + " Hit back button");
        BackBehavior?.Invoke();
    }

    public void OnReady(InputAction.CallbackContext ctxt)
    {
        if (debugMessages)
            Debug.Log("Player " + playerIndex + " Hit ready button");
        ReadyBehavior?.Invoke();
    }
}
