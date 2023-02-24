using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    // NOTE: If no sound is being played from this Sound object,
    // make sure that volume AND pitch are both greater than 0.

    public string name;

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

        if (play)
            source.Play();
    }

    public void RefreshSourceSettings()
    {
        source.playOnAwake = false;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
    }
}
