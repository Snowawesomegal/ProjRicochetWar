using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActivateHitbox : MonoBehaviour
{
    public Animator anim;
    public GameObject hitboxPrefab;
    public Control1 c1;
    public ControlLockManager clm;
    public Dictionary<GameObject, int> toBeDisabled = new Dictionary<GameObject, int>();



    public void CreateHitbox(string hitboxName)
    {
        var allKids = GetComponentsInChildren<Transform>();
        GameObject hitboxObject = allKids.Where(k => k.gameObject.name == hitboxName).FirstOrDefault().gameObject;

        hitboxObject.SetActive(true);

        HitboxInfo HBInfo = hitboxObject.GetComponent<HitboxInfo>();

        HBInfo.owner = gameObject;
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
            }
            else
            {
                toBeDisabled[i.Key] -= 1;
            }
        }
    }

    public void EndAnimation() //placeholder //TODO
    {
        clm.activeLockers.Remove(c1.inAnim);
        anim.SetBool("HeavyAttack", false);
    }
}
