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
            foreach (AbstractSound s in sounds)
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
            foreach (AbstractSound s in music)
            {
                s.volume = musicVolume;
                s.RefreshSourceSettings();
            }
        }
    }

    [SerializeField] public List<EffectSound> sounds;
    [SerializeField] public List<MusicSound> music;

    [HideInInspector] public Dictionary<string, EffectSound> soundMap;
    [HideInInspector] public Dictionary<string, SoundEffectGroup> soundGroupMap;
    [HideInInspector] public Dictionary<string, MusicSound> musicMap;

    [SerializeField] public GameObject soundSourceTarget;
    [HideInInspector] public AudioSource musicSource;
    [HideInInspector] public MusicSound currentMusic;

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
        PrepareMusicSoundSources();
        PrepareEffectSoundSources();
    }

    private void PrepareMusicSoundSources()
    {
        // prepare music source and map
        musicMap = new Dictionary<string, MusicSound>();
        musicSource = soundSourceTarget.AddComponent<AudioSource>();
        musicSource.ignoreListenerPause = true;
        if (music.Count > 0)
        {
            currentMusic = music[0];
            currentMusic.EstablishSource(musicSource, true);
        }
        foreach (MusicSound s in music)
        {
            musicMap[s.name] = s;
        }
    }

    private void PrepareEffectSoundSources()
    {
        // prepare sound sources and map
        soundMap = new Dictionary<string, EffectSound>();
        soundGroupMap = new Dictionary<string, SoundEffectGroup>();
        foreach (EffectSound s in sounds)
        {
            s.EstablishSource(soundSourceTarget.AddComponent<AudioSource>());
            soundMap[s.name] = s;
            if (s.TryGetGroupName(out string groupName))
            {
                if (soundGroupMap.TryGetValue(groupName, out SoundEffectGroup effectGroup))
                {
                    effectGroup.sounds.Add(s);
                } else
                {
                    Debug.Log("Making new sound effect group: " + groupName);
                    SoundEffectGroup newEffectGroup = new SoundEffectGroup();
                    newEffectGroup.sounds.Add(s);
                    soundGroupMap.Add(groupName, newEffectGroup);
                }
            }
        }
    }

    public bool PlaySound(string name)
    {
        if (soundMap.TryGetValue(name, out EffectSound sound))
        {
            sound.source.Play();
            return true;
        }
        Debug.LogWarning("Attempted to play Sound by name: " + name + ". Sound not found in AudioManager.");
        return false;
    }

    public bool PlaySoundGroup(string groupName)
    {
        if (soundGroupMap.TryGetValue(groupName, out SoundEffectGroup group))
        {
            group.PlaySound();
            return true;
        }
        Debug.LogWarning("Attempted to play Sound Group by name: " + name + ". Sound Group not found in AudioManager.");
        return false;
    }

    public bool SetMusic(string name)
    {
        if (musicMap.TryGetValue(name, out MusicSound sound))
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
