using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EffectSound : AbstractSound
{
    [SerializeField] public string groupName = "";
    [SerializeField] public float groupWeight = 1f;
    [SerializeField] public bool HasGroup { get { return !string.IsNullOrWhiteSpace(groupName); } }

    [SerializeField] public int loopsLeft = 0;
    private bool stopped = false;
    private AudioManager subscriber = null;

    public bool TryGetGroupName(out string name)
    {
        name = groupName;
        return HasGroup;
    }

    public override void OnEstablishSource()
    {
        // :)
    }

    public override void RefreshSourceSettings()
    {
        base.RefreshSourceSettings();
        source.loop = false;
    }

    public override void Play()
    {
        base.Play();
        stopped = false;
    }

    public override void Stop()
    {
        loopsLeft = 0;
        stopping = true;
    }

    public override void FinishStop()
    {
        base.FinishStop();
        loopsLeft = 0;
        stopped = true;
        if (subscriber)
        {
            subscriber.OnUpdateMusic -= UpdateLoop;
            subscriber = null;
        }
    }

    public void Play(AudioManager am)
    {
        Play();
        if (!subscriber)
        {
            am.OnUpdateMusic += UpdateLoop;
            subscriber = am;
        } else
        {
            subscriber.OnUpdateMusic -= UpdateLoop;
            am.OnUpdateMusic += UpdateLoop;
            subscriber = am;
        }
    }

    public override void UpdateLoop()
    {
        if (source == null)
            UnsubscribeUpdate();

        base.UpdateLoop();
        if ((!Application.isFocused && Application.isPlaying) || stopped)
            return;
        
        Stopping(); // run if the sound is being stopped

        if (!source.isPlaying)
        {
            if (loop)
            {
                Play();
            }
            else if (loopsLeft > 0)
            {
                loopsLeft--;
                Play();
            } else
            {
                UnsubscribeUpdate();
            }
        }
    }

    private void UnsubscribeUpdate()
    {
        if (subscriber)
        {
            subscriber.OnUpdateMusic -= UpdateLoop;
            subscriber = null;
            stopped = true;
        }
    }
}
