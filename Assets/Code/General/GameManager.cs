using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSlowing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public const int MENU_SCENE = 0;
    public const int PLAY_SCENE = 1;

    // This class follows a Singleton design pattern
    // a.k.a. EXACTLY ONE instance of this script should be included in the scene.
    // Only one and exactly one game object can have this component attached to it.
    // This static Instance property allows all other classes to easily grab this object
    // by calling GameManager.Instance in order to interact with it.
    private static GameManager instance;
    public static GameManager Instance { get { if (instance == null) instance = FindObjectOfType<GameManager>(); return instance; } }

    public TickingTimeController timeController;
    public TickingTimeController TimeController { get { if (timeController == null) timeController = new TickingTimeController(SlowUpdateType.FIXED); return timeController; } }

    public SessionSettings session;
    public SessionSettings Session { get { if (session == null) session = new SessionSettings(); return session; } }

    public static TickingTimeController InstanceTimeController { get { return Instance.TimeController; } }

    private void Awake()
    {
        // Make sure instance is set if it is not already
        if (instance == null)
        {
            instance = this;
        // check to see if another GameManager already assigned itself or got assigned
        } else if (instance != this)
        {
            Debug.LogWarning("ERROR - Duplicate GameManager instantiated... cannot set static instance to this GameManager. Destroying this GameManager...");
            Destroy(gameObject);
            return;
        }
        if (TimeController == null)
            Debug.LogWarning("TimeController isn't being properly created on awake.");

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += (scene, loadMode) =>
        {
            if (scene.buildIndex == PLAY_SCENE)
                SetupGame();
        };
        SceneManager.sceneUnloaded += (scene) =>
        {
            CleanupGame();
        };
    }

    private void FixedUpdate()
    {
        TimeController.Tick();
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    /// <returns> the current value of paused after pausing </returns>
    public bool PauseGame()
    {
        timeController.Frozen = true;
        return true;
    }

    /// <summary>
    /// Unpause the game.
    /// </summary>
    /// <returns> the current value of paused after unpausing </returns>
    public bool UnpauseGame()
    {
        timeController.Frozen = false;
        return false;
    }

    public void ResetGame()
    {
        CleanupGame();
        SetupGame();
    }

    public void CleanupGame()
    {
        timeController = new TickingTimeController(SlowUpdateType.FIXED);
        Session.DestroyAllPlayers();
    }

    public void SetupGame()
    {
        Session.SpawnAllPlayers(new Vector3[] { new Vector3(-6, 0, 0), new Vector3(6, 0, 0), new Vector3(-2, 0, 0), new Vector3(2, 0, 0) });
    }

    public void LoadGameScene()
    {
        Debug.Log("-----Switching to Play Scene-----");
        SceneManager.LoadScene(PLAY_SCENE);
    }

    public void LoadSelectScene()
    {
        Debug.Log("-----Switching to Character Select Menu-----");
        FighterSelectorMenu.shouldDisableOnStart = false;
        SceneManager.LoadScene(MENU_SCENE);
    }
}
