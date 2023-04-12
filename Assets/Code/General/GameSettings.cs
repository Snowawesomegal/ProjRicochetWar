using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class GameSettings : MonoBehaviour
{
    // Singleton design pattern
    private static GameSettings instance;
    public static GameSettings Instance { get { if (instance == null) instance = FindObjectOfType<GameSettings>(); return instance; } }

    #region Volume and Sound
    [Header("Volume")]
    [SerializeField] [Range(0, 1)] private float masterVolume = 0.3f;
    public float MasterVolume { get { return masterVolume; } set { masterVolume = Mathf.Clamp(value, 0, 1); } }

    [SerializeField] [Range(0, 1)] private float musicVolume = 1f;
    public float MusicVolume { get { return musicVolume; } set { musicVolume = Mathf.Clamp(value, 0, 1); } }

    [SerializeField] [Range(0, 1)] private float effectVolume = 1f;
    public float EffectVolume { get { return effectVolume; } set { effectVolume = Mathf.Clamp(value, 0, 1); } }

    [SerializeField] public AudioManager audioManager;
    #endregion

    private bool initialized = false;

    private void Awake()
    {
        VerifyInitialization();

        DontDestroyOnLoad(gameObject);
    }

    public void VerifyInitialization()
    {
        if (initialized)
            return;

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Error - GameSettings instance is already set... duplicate GameSettings exists. Destroying duplicate...");
            Destroy(gameObject);
            return;
        }
        if (audioManager == null)
        {
            audioManager = GetComponent<AudioManager>();
        }
        initialized = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SettingsUpdate();
    }

    public void SettingsUpdate()
    {
        UpdateAudioManager();
    }

    private void UpdateAudioManager()
    {
        float scaledMusicVolume = masterVolume * musicVolume;
        if (audioManager.MusicVolume != scaledMusicVolume)
        {
            audioManager.MusicVolume = scaledMusicVolume;
        }
        float scaledEffectVolume = masterVolume * effectVolume;
        if (audioManager.EffectVolume != scaledEffectVolume)
        {
            audioManager.EffectVolume = scaledEffectVolume;
        }
    }
}
