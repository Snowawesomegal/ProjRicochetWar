using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton design pattern - only 1 per scene
    private static AudioManager instance;
    public static AudioManager Instance { get { if (instance == null) instance = FindObjectOfType<AudioManager>(); return instance; } private set { instance = value; } }

    [SerializeField] private float effectVolume;
    public float EffectVolume { 
        get { return effectVolume; } 
        set
        {
            effectVolume = value;
            foreach (Sound s in sounds)
            {
                s.volume = effectVolume;
                s.RefreshSourceSettings();
            }
        }
    }

    [SerializeField] private float musicVolume;
    public float MusicVolume
    {
        get { return musicVolume; }
        set
        {
            musicVolume = value;
            foreach (Sound s in music)
            {
                s.volume = musicVolume;
                s.RefreshSourceSettings();
            }
        }
    }

    [SerializeField] public List<Sound> sounds;
    [SerializeField] public List<Sound> music;

    [HideInInspector] public Dictionary<string, Sound> soundMap;
    [HideInInspector] public Dictionary<string, Sound> musicMap;

    [SerializeField] public GameObject soundSourceTarget;
    [HideInInspector] public AudioSource musicSource;
    [HideInInspector] public Sound currentMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Debug.LogError("Error - AudioManager instance is already set... duplicate AudioManager exists. Destroying duplicate...");
            Destroy(gameObject);
            return;
        }
        if (soundSourceTarget == null)
        {
            soundSourceTarget = gameObject;
        }
        PrepareSoundSources();
    }

    private void PrepareSoundSources()
    {
        // prepare music source and map
        musicMap = new Dictionary<string, Sound>();
        musicSource = soundSourceTarget.AddComponent<AudioSource>();
        musicSource.ignoreListenerPause = true;
        if (music.Count > 0)
        {
            currentMusic = music[0];
            currentMusic.EstablishSource(musicSource, true);
        }
        foreach (Sound s in music)
        {
            musicMap[s.name] = s;
        }

        // prepare sound sources and map
        soundMap = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            s.EstablishSource(soundSourceTarget.AddComponent<AudioSource>());
            soundMap[s.name] = s;
        }
    }

    public bool PlaySound(string name)
    {
        if (soundMap.TryGetValue(name, out Sound sound))
        {
            return true;
        }
        Debug.LogWarning("Attempted to play Sound by name: " + name + ". Sound not found in AudioManager.");
        return false;
    }

    public bool SetMusic(string name)
    {
        if (musicMap.TryGetValue(name, out Sound sound))
        {
            sound.EstablishSource(musicSource, true);
            currentMusic = sound;
            return true;
        }
        Debug.LogWarning("Attempted to play music by name: " + name + ". Music Sound not found in AudioManager.");
        return false;
    }

    private void FixedUpdate()
    {
        currentMusic?.UpdateLoop();
    }
}
