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

    private void Start()
    {
        clm = GetComponent<ControlLockManager>();
        c1 = GetComponent<Control1>();
    }

    public void EnableHitbox(string hitboxName)
    {
        GameObject hitboxObject = null;

        if (hitboxName.Contains('/')) // have to find a parent and then a child within the parent (input is entered as parent/child if hitbox is nested)
        {
            string[] parentAndHB = hitboxName.Split('/'); // split and add to array

            foreach (Transform i in transform) // foreach child in player
            {
                if (i.name == parentAndHB[0]) // if that child is the parent we're looking for
                {
                    foreach (Transform j in i) // foreach child in that child
                    {
                        if (j.gameObject.name == parentAndHB[1]) // if that child is the child we're looking for
                        {
                            hitboxObject = j.gameObject;
                            break;
                        }
                    }
                    break;
                }
            }
        }
        else // finding non-nested child in player's transform
        {
            foreach (Transform i in transform)
            {
                if (i.name == hitboxName)
                {
                    hitboxObject = i.gameObject;
                    break;
                }
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

    public void EnableMultiHitbox(string hitboxParentName)
    {
        List<GameObject> hitboxesToEnable = new List<GameObject>();

        foreach (Transform i in transform)
        {
            if (i.name == hitboxParentName)
            {
                foreach (Transform j in i)
                {
                    hitboxesToEnable.Add(j.gameObject);
                }

                break;
            }
        }

        if (hitboxesToEnable.Count == 0)
        {
            Debug.Log("There is no hitbox parent with the name " + hitboxParentName + " on " + gameObject.name);
            return;
        }

        foreach (GameObject i in hitboxesToEnable)
        {
            i.SetActive(true);

            HitboxInfo HBInfo = i.GetComponent<HitboxInfo>();

            HBInfo.owner = gameObject;

            if (HBInfo.owner.GetComponent<Control1>().facingRight != HBInfo.facingRight)
            {
                i.transform.localPosition *= new Vector2(-1, 1);
            }
            HBInfo.facingRight = HBInfo.owner.GetComponent<Control1>().facingRight;

            toBeDisabled.Add(i, HBInfo.activeFrames);
        }
    }

    private void FixedUpdate()
    {
        Dictionary<GameObject, int> copyDict = new Dictionary<GameObject, int>(toBeDisabled);

        foreach(KeyValuePair<GameObject, int> i in copyDict)
        {
            if (i.Value <= 0)
            {
                i.Key.SetActive(false);
                i.Key.GetComponent<HitboxInfo>().playersHitAlready.Clear();
                toBeDisabled.Remove(i.Key);
            }
            else
            {
                toBeDisabled[i.Key] -= 1;
            }
        }
    }
}
