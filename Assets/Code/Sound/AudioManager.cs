using UnityEngine.Audio;
using UnityEngine;
using UnityEditor;
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
                s.volume = effectVolume * s.presetVolume;
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
                s.volume = musicVolume * s.presetVolume;
                s.RefreshSourceSettings();
            }
        }
    }

    [SerializeField] public List<EffectSound> sounds;
    [SerializeField] public List<MusicSound> music;

    [HideInInspector] public Dictionary<string, EffectSound> soundMap;
    [HideInInspector] public Dictionary<string, SoundEffectGroup> soundGroupMap;
    [HideInInspector] public Dictionary<string, MusicSound> musicMap;

    [SerializeField] public GameObject soundSourceReferenceTarget;
    [SerializeField] public GameObject soundSourceTarget;
    [HideInInspector] public AudioSource musicSource;
    [HideInInspector] public MusicSound currentMusic;

    public event System.Action OnUpdateMusic;

    protected bool prepared = false;
    public bool Prepared { get { PrepareSoundSources(); return prepared; } set { prepared = value; if (!prepared) { ReprepareSoundSources(); } } }

    private void Awake()
    {
        ForcePrepareSoundSources();
        if (!prepared)
        {
            Debug.LogError("Audio Manager not prepared - Likely missing SoundSourceReferenceTarget -- reference is needed to create audio source target.");
        }
    }

    private void ForcePrepareSoundSources()
    {
        if (prepared)
            ReprepareSoundSources();
        else
            PrepareSoundSources();
    }

    private void ReprepareSoundSources()
    {
        if (Application.isPlaying)
            Destroy(soundSourceTarget);
        else
            DestroyImmediate(soundSourceTarget);
        soundSourceTarget = null;
        prepared = false;
        PrepareSoundSources();
    }

    private void PrepareSoundSources()
    {
        if (prepared)
            return;

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Error - AudioManager instance is already set... duplicate AudioManager exists. Destroying duplicate...");
            Destroy(gameObject);
            return;
        }
        if (soundSourceReferenceTarget == null)
        {
            return;
        }
        else
        {
            soundSourceTarget = Instantiate(soundSourceReferenceTarget, transform);
            soundSourceTarget.name = "Sound Source Target (Don't Inspect)";
        }
        PrepareMusicSoundSources();
        PrepareEffectSoundSources();
        prepared = true;
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
                    SoundEffectGroup newEffectGroup = new SoundEffectGroup(groupName);
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
            sound.Play(this);
            return true;
        }
        Debug.LogWarning("Attempted to play Sound by name: " + name + ". Sound not found in AudioManager.");
        return false;
    }

    public bool AddRepeats(string name, int repeats)
    {
        if (soundMap.TryGetValue(name, out EffectSound sound))
        {
            sound.loopsLeft += repeats;
            if (!sound.source.isPlaying)
            {
                sound.Play(this);
            }
            return true;
        }
        Debug.LogWarning("Attempted to add repeats to Sound by name: " + name + ". Sound not found in AudioManager.");
        return false;
    }

    public bool StopSound(string name)
    {
        if (soundMap.TryGetValue(name, out EffectSound sound))
        {
            sound.Stop();
            return true;
        }
        Debug.LogWarning("Attempted to stop Sound by name: " + name + ". Sound not found in AudioManager.");
        return false;
    }

    public bool PlaySoundGroup(string groupName)
    {
        if (soundGroupMap.TryGetValue(groupName, out SoundEffectGroup group))
        {
            group.PlaySound(this);
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

    public bool StopMusic()
    {
        currentMusic?.source.Stop();
        currentMusic = null;
        return true;
    }

    private void FixedUpdate()
    {
        UpdateSounds();
    }

    public void UpdateSounds()
    {
        if (!prepared)
            return;
        currentMusic?.UpdateLoop();
        OnUpdateMusic?.Invoke();
    }

    private void OnEnable()
    {
        EditorApplication.update += () => { if (Application.isEditor) UpdateSounds(); };
    }
}
