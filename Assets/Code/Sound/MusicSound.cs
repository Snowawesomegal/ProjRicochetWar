using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MusicSound : AbstractSound
{
    [HideInInspector] public bool looped = false;
    [SerializeField] public float loopStart = 0f;
    [SerializeField] public float loopEnd = 0f;
    private bool stopped = false;

    public override void OnEstablishSource()
    {
        looped = false;
    }

    public override void RefreshSourceSettings()
    {
        base.RefreshSourceSettings();
        source.loop = loop && (loopStart == 0 && loopEnd == 0);
    }

    public override void Play()
    {
        if (!loop)
        {
            source.Play();
        }
        else
        {
            if (!looped)
            {
                looped = true;
                PlayInterval(0, loopEnd);
            }
            else
            {
                PlayInterval(loopStart, loopEnd);
            }
        }
        stopped = false;
    }

    public override void Stop()
    {
        base.Stop();
        stopped = true;
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

    public override void UpdateLoop()
    {
        if (stopped || !loop || !Application.isFocused)
            return;

        if (!source.isPlaying)
        {
            Play();
        }
    }
}
