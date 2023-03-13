using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    GameObject sm;
    AudioManager am;

    [SerializeField] List<VisualEffect> effects = new List<VisualEffect>(); // see VisualEffect class at bottom of this script

    private void Start()
    {
        sm = GameObject.Find("SettingsManager");
        am = sm.GetComponent<AudioManager>();
    }

    public void SpawnDirectionalEffect(string effectName, Vector3 position, bool facingRight, float xdisplacement = 0, float ydisplacement = 0)
    {
        VisualEffect effect = GetVisualEffect(effectName);

        if (!string.IsNullOrEmpty(effect.sound))
        {
            am.PlaySound(effect.sound);
        }

        GameObject newEffect = Instantiate(effect.prefab, position + new Vector3(-0.5f * (facingRight ? 1 : -1) + xdisplacement, ydisplacement, 0), Quaternion.identity);
        newEffect.GetComponent<SpriteRenderer>().flipX = !facingRight;
        Destroy(newEffect, effect.lifetime);
    }


    public void SpawnHitEffectOnContactPoint(string effectName, Collider2D hitbox, Vector2 hurtboxCenter, float angleAdjustment = 0, float xdisplacement = 0, float ydisplacement = 0)
    {
        VisualEffect effect = GetVisualEffect(effectName);

        if (!string.IsNullOrEmpty(effect.sound))
        {
            am.PlaySound(effect.sound);
        }

        float angle = hitbox.GetComponent<HitboxInfo>().angle;
        Quaternion rotation = Quaternion.Euler(0, 0, angle + angleAdjustment + effect.rotationAdjustment);

        Vector3 position = hitbox.ClosestPoint(hurtboxCenter);

        GameObject newEffect = Instantiate(effect.prefab, position + new Vector3(xdisplacement, ydisplacement, 0), rotation);

        Destroy(newEffect, effect.lifetime);
    }

    VisualEffect GetVisualEffect(string name)
    {
        foreach (VisualEffect effect in effects)
        {
            if (effect.name == name)
            {
                return effect;
            }
        }

        Debug.Log("There is no visual effect named " + name + " in EffectManager's inspector.");
        return null;
    }
}

[System.Serializable]
public class VisualEffect
{
    public string name;
    public float lifetime;
    public GameObject prefab;
    public string sound;
    public float rotationAdjustment;
}
