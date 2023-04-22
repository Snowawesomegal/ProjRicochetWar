using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [SerializeField] Slider master;
    [SerializeField] Slider music;
    [SerializeField] Slider sfx;

    public void ChangeMaster()
    {
        GameSettings.Instance.MasterVolume = master.value;
    }

    public void ChangeMusic()
    {
        GameSettings.Instance.MusicVolume = music.value;
    }

    public void ChangeSFX()
    {
        GameSettings.Instance.EffectVolume = sfx.value;
    }
}
