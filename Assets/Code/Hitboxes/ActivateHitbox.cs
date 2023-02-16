using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateHitbox : MonoBehaviour
{
    public Animator anim;
    public GameObject hitboxPrefab;
    public Control1 c1;
    public ControlLockManager clm;
    Dictionary<GameObject, int> toBeDestroyed = new Dictionary<GameObject, int>();


    public void CreateHitbox(string hitboxPath)
    {
        ScriptableHitbox hitboxData = (ScriptableHitbox) Resources.Load(hitboxPath);

        GameObject newHitbox = Instantiate(hitboxPrefab, transform.position, Quaternion.identity);

        CapsuleCollider2D hbCollider = newHitbox.GetComponent<CapsuleCollider2D>();
        HitboxInfo newHBInfo = newHitbox.GetComponent<HitboxInfo>();

        hbCollider.offset = new Vector2(hitboxData.xoffset, hitboxData.yoffset);
        hbCollider.direction = hitboxData.changeToVertical ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
        hbCollider.size = new Vector2(hitboxData.xsize, hitboxData.ysize);

        newHBInfo.damage = hitboxData.damage;
        newHBInfo.knockback = hitboxData.knockback;
        newHBInfo.angle = hitboxData.kbAngleNeg90to90;
        newHBInfo.angleIndependentOfMovement = hitboxData.angleIndependentOfMovement;
        newHBInfo.owner = gameObject;
        newHBInfo.facingRight = newHBInfo.owner.GetComponent<Control1>().facingRight;

        toBeDestroyed.Add(newHitbox, hitboxData.activeFrames);
    }

    private void FixedUpdate()
    {
        Dictionary<GameObject, int> copyDict = new Dictionary<GameObject, int>(toBeDestroyed);

        foreach(KeyValuePair<GameObject, int> i in copyDict)
        {
            if (i.Value <= 0)
            {
                Destroy(i.Key);
            }
            else
            {
                toBeDestroyed[i.Key] -= 1;
            }
        }
    }

    public void EndAnimation() //placeholder
    {
        clm.activeLockers.Remove(c1.inAnim);
        anim.SetBool("HeavyAttack", false);
    }
}
