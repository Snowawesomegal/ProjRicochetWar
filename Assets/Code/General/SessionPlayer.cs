using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionPlayer
{
    private int playerIndex;
    private PlayerInput playerInput;
    public int PlayerIndex { get { return playerIndex; } }
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
        this.playerInput = playerInput;
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
        instancedInput = PlayerInput.Instantiate(selectedFighter.fighter.gameObject, playerIndex, playerInput.currentControlScheme, -1, playerInput.devices.ToArray());
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
