using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSettings : MonoBehaviour
{
    public void ChangeMaster(float v)
    {
        GameSettings.Instance.MasterVolume = v;
    }

    public void ChangeMusic(float v)
    {
        GameSettings.Instance.MusicVolume = v;
    }

    public void ChangeSFX(float v)
    {
        GameSettings.Instance.EffectVolume = v;
    }
}
