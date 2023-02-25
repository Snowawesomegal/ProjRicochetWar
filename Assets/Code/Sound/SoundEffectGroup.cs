using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectGroup
{
    public string groupName;

    public List<EffectSound> sounds = new List<EffectSound>();
    
    public bool PlaySound()
    {
        //Debug.Log("Playing sound from group.");
        float totalWeight = 0f;
        List<EffectSound> choices = new List<EffectSound>();
        foreach (EffectSound sound in sounds)
        {
            if (!sound.source.isPlaying)
            {
                totalWeight += sound.groupWeight;
                choices.Add(sound);
            }
        }

        if (choices.Count == 0)
            return ForcePlaySound();

        float selection = Random.Range(0, totalWeight);
        float current = 0;
        EffectSound playChoice = null;
        foreach (EffectSound sound in choices)
        {
            current += sound.groupWeight;
            if (current >= selection)
            {
                playChoice = sound;
                break;
            }
        }
        //Debug.Log("Sound selected from group: " + (playChoice != null ? playChoice.name : "choice is null with selection val: " + selection));
        playChoice?.Play();
        return playChoice != null;
    }

    public bool ForcePlaySound()
    {
        //Debug.Log("Forcing sound to be played from some currently playing sound.");
        float totalWeight = 0f;
        foreach (EffectSound sound in sounds)
        {
            totalWeight += sound.groupWeight;
        }

        float selection = Random.Range(0, totalWeight);
        float current = 0;
        EffectSound playChoice = null;
        foreach (EffectSound sound in sounds)
        {
            current += sound.groupWeight;
            if (current >= selection)
            {
                playChoice = sound;
                break;
            }
        }
        playChoice?.Play();
        return playChoice != null;
    }
}
