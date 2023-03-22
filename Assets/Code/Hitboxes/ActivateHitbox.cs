using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System;
using System.Xml.Linq;

public class ActivateHitbox : MonoBehaviour
{
    public Animator anim;
    public GameObject hitboxPrefab;
    Control1 c1;
    ControlLockManager clm;

    public Dictionary<Pair<GameObject, Type>, int> toBeDisabled = new Dictionary<Pair<GameObject, Type>, int>();

    private void Start()
    {
        clm = GetComponent<ControlLockManager>();
        c1 = GetComponent<Control1>();
    }

    /// <summary>
    /// Finds by name and enables any box that is a direct child of the object this script is attached to. This function also works for pogoboxes. For any nested children, which should be the case for multipart, multihitbox, and multihit attacks, use EnableMultiHitbox. 
    /// </summary>
    /// <param name="hitboxName"></param>
    public void EnableHitbox(string boxName)
    {
        GameObject boxObject = null;

        if (boxName.Contains('/')) // have to find a parent and then a child within the parent (input is entered as parent/child if hitbox is nested)
        {
            string[] parentAndHB = boxName.Split('/'); // split and add to array

            foreach (Transform i in transform) // foreach child in player
            {
                if (i.name == parentAndHB[0]) // if that child is the parent we're looking for
                {
                    foreach (Transform j in i) // foreach child in that child
                    {
                        if (j.gameObject.name == parentAndHB[1]) // if that child is the child we're looking for
                        {
                            boxObject = j.gameObject;
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
                if (i.name == boxName)
                {
                    boxObject = i.gameObject;
                    break;
                }
            }
        }

        if (boxObject == null)
        {
            Debug.Log("There is no hitbox object with the name " + boxName + " on " + gameObject.name);
            return;
        }

        if (boxObject.TryGetComponent(out HitboxInfo hbi))
        {
            if (!hbi.doNotEnable)
            {
                boxObject.SetActive(true);
                CalibrateBox(boxObject);
            }
        }
        else
        {
            boxObject.SetActive(true);
            CalibrateBox(boxObject);
        }
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
            if (i.TryGetComponent(out HitboxInfo hbi))
            {
                if (!hbi.doNotEnable)
                {
                    i.SetActive(true);
                    CalibrateBox(i);
                }
            }
            else
            {
                i.SetActive(true);
            }
        }
    }

    void CalibrateBox(GameObject box)
    {
        if (box.TryGetComponent<HitboxInfo>(out HitboxInfo HBInfo))
        {
            CalibrateHitbox(HBInfo);
        }
        else if (box.TryGetComponent<Pogobox>(out Pogobox pogobox))
        {
            CalibratePogobox(pogobox);
        }
        else
        {
            CalibrateDefault(box);
        }

        void CalibrateHitbox(HitboxInfo HBInfo)
        {
            HBInfo.owner = gameObject;

            if (HBInfo.owner.GetComponent<Control1>().facingRight != HBInfo.facingRight)
            {
                HBInfo.transform.localPosition *= new Vector2(-1, 1);
            }
            HBInfo.facingRight = HBInfo.owner.GetComponent<Control1>().facingRight;

            toBeDisabled.Add(new Pair<GameObject, Type>(HBInfo.gameObject, typeof(HitboxInfo)), HBInfo.activeFrames);
        }

        void CalibratePogobox(Pogobox pogobox)
        {
            if (pogobox.owner.GetComponent<Control1>().facingRight != pogobox.facingRight)
            {
                pogobox.transform.localPosition *= new Vector2(-1, 1);
            }
            pogobox.facingRight = pogobox.owner.GetComponent<Control1>().facingRight;

            toBeDisabled.Add(new Pair<GameObject, Type>(pogobox.gameObject, typeof(Pogobox)), pogobox.activeFrames);
        }

        void CalibrateDefault(GameObject box)
        {
            toBeDisabled.Add(new Pair<GameObject, Type>(box, null), 2);
        }
    }



    private void FixedUpdate()
    {
        Dictionary<Pair<GameObject, Type>, int> copyDict = new Dictionary<Pair<GameObject, Type>, int>(toBeDisabled); // this feels like it's very cost inefficient?

        foreach(KeyValuePair<Pair<GameObject, Type>, int> i in copyDict)
        {
            if (i.Value <= 0)
            {
                i.Key.left.SetActive(false);

                if (i.Key.right == typeof(HitboxInfo))
                {
                    i.Key.left.GetComponent<HitboxInfo>().playersHitAlready.Clear();
                }

                toBeDisabled.Remove(i.Key);
            }
            else
            {
                toBeDisabled[i.Key] -= 1;
            }
        }
    }
}
