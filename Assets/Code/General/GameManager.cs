using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSlowing;

public class GameManager : MonoBehaviour
{
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
            Debug.LogError("ERROR - Duplicate GameManager instantiated... cannot set static instance to this GameManager. Destroying this GameManager...");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
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
        timeController = new TickingTimeController(SlowUpdateType.FIXED);
    }
}
