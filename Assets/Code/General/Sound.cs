using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    // NOTE: If no sound is being played from this Sound object,
    // make sure that volume AND pitch are both greater than 0.

    [SerializeField] public string name;

    [SerializeField] public AudioClip clip;

    [SerializeField] [Range(0, 1)] public float volume = 1f;
    [SerializeField] [Range(0.1f, 3)] public float pitch = 1f;

    [SerializeField] public bool loop = false;
    [HideInInspector] public bool looped = false;
    [SerializeField] public float loopStart = 0f;
    [SerializeField] public float loopEnd = 0f;

    [HideInInspector] public AudioSource source;

    public void EstablishSource(AudioSource source) { EstablishSource(source, false); }
    public void EstablishSource(AudioSource source, bool play)
    {
        this.source = source;
        source.clip = clip;
        looped = false;
        RefreshSourceSettings();

        if (play)
            Play();
    }

    public void RefreshSourceSettings()
    {
        source.playOnAwake = false;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop && (loopStart == 0 && loopEnd == 0);
    }

    public void Play()
    {
        if (!loop)
        {
            source.Play();
        } else
        {
            if (!looped)
            {
                looped = true;
                PlayInterval(0, loopEnd);
            } else
            {
                PlayInterval(loopStart, loopEnd);
            }
        }
    }

    public void PlayInterval(float from, float to)
    {
        if (from < 0)
            from = 0;
        if (to < 0)
            to = 0;
        if (to > 0 && to < from)
            to = clip.length;

        source.time = from;
        source.Play();
        if (to > 0)
        {
            double clipTime = to - from;
            source.SetScheduledEndTime(AudioSettings.dspTime + clipTime);
        }
    }

    public void UpdateLoop()
    {
        if (!loop || !Application.isFocused)
            return;

        if (!source.isPlaying)
        {
            Play();
        }
    }
}
