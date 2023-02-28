using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public abstract class AbstractSound
{
    // NOTE: If no sound is being played from this Sound object,
    // make sure that volume AND pitch are both greater than 0.

    [SerializeField] public string name;

    [SerializeField] public AudioClip clip;

    [SerializeField] [Range(0, 1)] public float volume = 1f;
    [SerializeField] [Range(0.1f, 3)] public float pitch = 1f;

    [SerializeField] public bool loop = false;

    [HideInInspector] public AudioSource source;

    public void EstablishSource(AudioSource source) { EstablishSource(source, false); }
    public void EstablishSource(AudioSource source, bool play)
    {
        this.source = source;
        source.clip = clip;
        RefreshSourceSettings();
        OnEstablishSource();

        if (play)
            Play();
    }

    public abstract void OnEstablishSource();

    public virtual void RefreshSourceSettings()
    {
        source.playOnAwake = false;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
    }

    public virtual void Play()
    {
        source.Play();
    }

    public virtual void UpdateLoop()
    {
        if (pitch == 0)
            pitch = 1;
        // :)
    }
}
