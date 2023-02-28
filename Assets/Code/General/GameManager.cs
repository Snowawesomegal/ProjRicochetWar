using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // This class follows a Singleton design pattern
    // a.k.a. EXACTLY ONE instance of this script should be included in the scene.
    // Only one and exactly one game object can have this component attached to it.
    // This static Instance property allows all other classes to easily grab this object
    // by calling GameManager.Instance in order to interact with it.
    private static GameManager instance;
    public static GameManager Instance { get { if (instance == null) instance = FindObjectOfType<GameManager>(); return instance; } }

    // instance variable and property of paused
    private bool paused = false;
    public bool Paused
    {
        get { return paused; }
        set
        {
            // when paused is changed to another value
            if (value != paused)
            {
                paused = value; // update the value
                OnPause?.Invoke(this, paused); // invoke all OnPause events
            }
        }
    }

    // This is a delegate used to create the OnPause event. In order to subscribe to the OnPause event,
    // the method being subscribed must follow this signature (a.k.a. return void and receive a GameManager and bool as params)
    // The GameManager passed in will be the singleton instance of the GameManager. The boolean being passed in will be
    // the current value of paused.
    public delegate void BoolGameManagerEvent(GameManager sender, bool b);
    // This is the event property. To subscribe a method to this event, use this command:
    // GameManager.Instance.OnPause += MyMethod; // note that MyMethod is a BoolGameManagerEvent delegate
    // To unsubscribe to this event, use this command:
    // GameManager.Instance.OnPause -= MyMethod;
    // Subscribing to this event allows objects to trigger a function whenever the game is paused or unpaused.
    public event BoolGameManagerEvent OnPause;

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
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    /// <returns> the current value of paused after pausing </returns>
    public bool PauseGame()
    {
        Paused = true;
        return Paused;
    }

    /// <summary>
    /// Unpause the game.
    /// </summary>
    /// <returns> the current value of paused after unpausing </returns>
    public bool UnpauseGame()
    {
        Paused = false;
        return Paused;
    }

    /// <summary>
    /// Toggle the pause value of the game. If the game is paused, it will unpause.
    /// If the game is unpaused, it will pause.
    /// </summary>
    /// <returns> the current value of paused after toggling </returns>
    public bool TogglePause()
    {
        Paused = !Paused;
        return Paused;
    }
}
