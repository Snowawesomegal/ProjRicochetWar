using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoiner : MonoBehaviour
{
    UnityEngine.InputSystem.PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = UnityEngine.InputSystem.PlayerInputManager.instance;

        inputManager.onPlayerJoined += AddPlayer;
    }

    public void AddPlayer(PlayerInput input)
    {
        GameManager.Instance.Session.AddPlayer(input);
    }
}
