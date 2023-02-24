using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton design pattern - only 1 per scene
    private AudioManager instance;
    public AudioManager Instance { get { if (instance == null) instance = FindObjectOfType<AudioManager>(); return instance; } private set { instance = value; } }

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

    [SerializeField] public Sound[] sounds;
    [SerializeField] public Sound[] music;

    [HideInInspector] public Dictionary<string, Sound> soundMap;
    [HideInInspector] public Dictionary<string, Sound> musicMap;

    [SerializeField] public GameObject soundSourceTarget;
    [HideInInspector] public AudioSource musicSource;

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
        if (music.Length > 0)
            music[0].EstablishSource(musicSource, true);
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
            sound.source.Play();
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
            return true;
        }
        Debug.LogWarning("Attempted to play music by name: " + name + ". Music Sound not found in AudioManager.");
        return false;
    }
}
