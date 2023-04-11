using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoiner : MonoBehaviour
{
    [SerializeField] public UnityEngine.InputSystem.PlayerInputManager inputManager;

    private void Awake()
    {
        if (inputManager == null)
            inputManager = GetComponent<UnityEngine.InputSystem.PlayerInputManager>();
        inputManager = UnityEngine.InputSystem.PlayerInputManager.instance;
        inputManager.onPlayerJoined += AddPlayer;
    }

    public void AddPlayer(PlayerInput input)
    {
        GameManager.Instance.Session.AddPlayer(input);
    }
}
