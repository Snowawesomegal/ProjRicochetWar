using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterSelectionManager : MonoBehaviour
{
    [SerializeField] public Sprite defaultSelectionImage;
    [SerializeField] public List<FighterSelectionDisplay> possiblePlayers = new List<FighterSelectionDisplay>();
    [SerializeField] public List<SessionPlayer> players = new List<SessionPlayer>();
    [SerializeField] public UnityEngine.InputSystem.PlayerInputManager inputManager;
    [SerializeField] public List<FighterSelectorButton> possibleButtons = new List<FighterSelectorButton>();

    private void Awake()
    {
        if (inputManager == null)
            inputManager = GetComponent<UnityEngine.InputSystem.PlayerInputManager>();
        inputManager = UnityEngine.InputSystem.PlayerInputManager.instance;
        inputManager.onPlayerJoined += AddPlayer;
        inputManager.onPlayerLeft += RemovePlayer;
    }

    private void OnEnable()
    {
        if (HasPlayerSlots())
            inputManager.EnableJoining();
        else
            inputManager.DisableJoining();
    }

    public bool HasPlayerSlots()
    {
        foreach (FighterSelectionDisplay selection in possiblePlayers)
            if (selection.Unassigned)
                return true;
        return false;
    }

    public void AddPlayer(PlayerInput input)
    {
        Debug.Log("Adding player! Input index: " + input.playerIndex);
        SessionPlayer player = GameManager.Instance.Session.AddPlayer(input);
        Debug.Log("Checking for next available player slot...");
        foreach (FighterSelectionDisplay selection in possiblePlayers)
        {
            if (selection.Unassigned)
            {
                Debug.Log("Found player slot! Assigning player...");
                selection.AssignPlayer(player, defaultSelectionImage, this);
                players.Add(player);
                break;
            }
        }
        if (!HasPlayerSlots())
            inputManager.DisableJoining();
    }

    public void RemovePlayer(PlayerInput input)
    {
        if (HasPlayerSlots())
            inputManager.EnableJoining();
    }
}
