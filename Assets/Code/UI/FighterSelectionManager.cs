using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterSelectionManager : MonoBehaviour
{
    [SerializeField] public Sprite defaultSelectionImage;
    [SerializeField] public List<FighterSelectionDisplay> possiblePlayers = new List<FighterSelectionDisplay>();
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
        {
            inputManager.EnableJoining();
            Debug.Log("Joining is enabled.");
        }
        else
        {
            inputManager.DisableJoining();
            Debug.Log("Joining is disabled.");
        }
    }

    private void OnDisable()
    {
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
        Debug.Log("Adding new player...");
        SessionPlayer player = GameManager.Instance.Session.AddPlayer(input);
        bool createdPlayer = AssignPlayer(player);
        if (!createdPlayer)
        {
            GameManager.Instance.Session.RemovePlayer(player.PlayerIndex);
            return;
        }
        if (!HasPlayerSlots())
        {
            inputManager.DisableJoining();
            Debug.Log("Disabled joining, maximum player count reached.");
        }
    }

    public bool AssignPlayer(SessionPlayer player)
    {
        foreach (FighterSelectionDisplay selection in possiblePlayers)
        {
            if (selection.Unassigned)
            {
                Debug.Log("Assigning player to new player slot!");
                selection.AssignPlayer(player, defaultSelectionImage, this);
                return true;
            }
        }
        return false;
    }

    public void RemovePlayer(PlayerInput input)
    {
        if (HasPlayerSlots())
        {
            inputManager.EnableJoining();
            Debug.Log("Enabled joining, player slot(s) are now available.");
        }
    }

    public void ClearPlayers()
    {
        foreach (FighterSelectionDisplay selection in possiblePlayers)
        {
            if (selection.HasPlayer)
                selection.RemovePlayer();
        }
    }

    public void QueryReady()
    {
        bool notReady = false;
        foreach (FighterSelectionDisplay fighter in possiblePlayers)
        {
            if (fighter.HasPlayer && !fighter.Ready)
            {
                notReady = true;
                break;
            }
        }
        if (notReady)
            return;

        GameManager.Instance.LoadGameScene();
    }

    public void CollectSessionPlayers()
    {
        ClearPlayers();
        inputManager.onPlayerJoined -= AddPlayer;
        SessionPlayer[] currentPlayers = GameManager.Instance.Session.players.ToArray();
        foreach (SessionPlayer player in currentPlayers)
        {
            string devices = "Devices {";
            foreach (InputDevice device in player.PlayerDevices)
            {
                devices += device.ToString() + ", ";
            }
            devices = devices.Length > 9 ? devices.Substring(0, devices.Length - 2) + "}" : devices + " }";
            Debug.Log("-----Found existing session player, reassigning player: " + devices);
            PlayerInput input = PlayerInput.Instantiate(inputManager.playerPrefab, player.PlayerIndex, player.PlayerControlScheme, player.PlayerIndex, player.PlayerDevices);
            GameManager.Instance.Session.RemovePlayer(player.PlayerIndex);
            AddPlayer(input);
        }
        inputManager.onPlayerJoined += AddPlayer;
    }
}
