using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System;

public class ActivateHitbox : MonoBehaviour
{
    public Animator anim;
    public GameObject hitboxPrefab;
    Control1 c1;
    ControlLockManager clm;

    public Dictionary<GameObject, int> toBeDisabled = new Dictionary<GameObject, int>();
    public List<GameObject> currentConnectedHitboxes;

    private void Start()
    {
        clm = GetComponent<ControlLockManager>();
        c1 = GetComponent<Control1>();
        currentConnectedHitboxes = new List<GameObject>() { };
    }

    public void ConnectHitboxes(string hitboxesSeparatedBySlashes)
    {
        string[] boxes = hitboxesSeparatedBySlashes.Split('/');

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform currentChild = transform.GetChild(i);
            if (Array.Exists(boxes, element => element == currentChild.name))
            {
                currentConnectedHitboxes.Add(currentChild.gameObject);
                break;
            }
        }
    }

    public void EnableHitbox(string hitboxName)
    {
        GameObject hitboxObject = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform currentChild = transform.GetChild(i);
            if (currentChild.name == hitboxName)
            {
                hitboxObject = currentChild.gameObject;
                break;
            }
        }

        if (hitboxObject == null)
        {
            Debug.Log("There is no hitbox object with the name " + hitboxName + " on " + gameObject.name);
            return;
        }

        hitboxObject.SetActive(true);

        HitboxInfo HBInfo = hitboxObject.GetComponent<HitboxInfo>();

        HBInfo.owner = gameObject;

        if (HBInfo.owner.GetComponent<Control1>().facingRight != HBInfo.facingRight)
        {
            hitboxObject.transform.localPosition *= new Vector2(-1, 1);
        }
        HBInfo.facingRight = HBInfo.owner.GetComponent<Control1>().facingRight;

        toBeDisabled.Add(hitboxObject, HBInfo.activeFrames);
    }

    private void FixedUpdate()
    {
        Dictionary<GameObject, int> copyDict = new Dictionary<GameObject, int>(toBeDisabled);

        foreach(KeyValuePair<GameObject, int> i in copyDict)
        {
            if (i.Value <= 0)
            {
                i.Key.SetActive(false);
                i.Key.GetComponent<HitboxInfo>().activeHitbox = true;
                toBeDisabled.Remove(i.Key);
            }
            else
            {
                toBeDisabled[i.Key] -= 1;
            }
        }
    }
}
