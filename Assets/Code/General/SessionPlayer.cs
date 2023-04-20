using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionPlayer
{
    private int playerIndex;
    private PlayerInput playerInput;
    private string playerControlScheme;
    private InputDevice[] playerDevices;
    public int PlayerIndex { get { return playerIndex; } }
    public string PlayerControlScheme {  get { return playerControlScheme; } }
    public InputDevice[] PlayerDevices {  get { return playerDevices; } }
    public PlayerInput Input { get { return playerInput; } }
    
    private FighterSelection selectedFighter = null;
    public FighterSelection SelectedFighter { get { return selectedFighter; } set { if (instancedInput == null && value != null) selectedFighter = value; } }
    
    private Control1 fighterInstance = null;
    private PlayerInput instancedInput = null;
    public Control1 FighterInstance { get { return fighterInstance; } }
    public PlayerInput InputInstance { get { return instancedInput; } }

    public SessionPlayer(PlayerInput playerInput)
    {
        this.playerIndex = playerInput.playerIndex;
        this.playerControlScheme = playerInput.currentControlScheme;
        this.playerDevices = playerInput.devices.ToArray();
        this.playerInput = playerInput;
        Debug.Log("Player " + playerIndex + ": " + playerInput.devices);
        foreach (InputDevice device in playerInput.devices)
        {
            Debug.Log("Device: " + device);
        }
    }
    public bool SpawnPlayer(Vector3 position)
    {
        if (fighterInstance != null)
        {
            Debug.LogError("Can't spawn player -- instance of fighter is already spawned.");
            return false;
        }

        if (selectedFighter == null)
        {
            Debug.LogError("Can't spawn player -- selected fighter is null.");
            return false;
        }

        Debug.Log("Spawning player and establishing instanced input.");
        instancedInput = PlayerInput.Instantiate(selectedFighter.fighter.gameObject, playerIndex, playerControlScheme, playerIndex, playerDevices);
        fighterInstance = instancedInput.GetComponent<Control1>();
        fighterInstance.transform.position = position;

        return true;
    }

    public bool DestroyPlayer()
    {
        if (fighterInstance != null)
        {
            GameObject.Destroy(fighterInstance.gameObject);
            fighterInstance = null;
            instancedInput = null;
            return true;
        }

        return false;
    }
}
