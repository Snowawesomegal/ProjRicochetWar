using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HitboxInteractionManager : MonoBehaviour
{
    EffectManager em;

    public List<Pair<Collider2D, Collider2D>> hitboxPlayerCol = new List<Pair<Collider2D, Collider2D>>();
    public List<Pair<Collider2D, Collider2D>> hitboxHitboxCol = new List<Pair<Collider2D, Collider2D>>();
    public List<Pair<Collider2D, Collider2D>> hitboxCounterCol = new List<Pair<Collider2D, Collider2D>>();
    public List<Pair<Collider2D, Collider2D>> grabPlayerCol = new List<Pair<Collider2D, Collider2D>>();

    public List<GameObject> doNotEnableHitboxes = new List<GameObject>();

    public float hitboxDamageDifferenceToWin = 10;

    public event Action<GameObject> SelfDestructObject;

    // Start is called before the first frame update
    void Start()
    {
        em = GameObject.Find("SettingsManager").GetComponent<EffectManager>();
        StartCoroutine(nameof(AfterPhysicsUpdate));
    }

    void AddPlayerToConnectedHitboxes(GameObject hb, GameObject player)
    {
        HitboxInfo hbi = hb.GetComponent<HitboxInfo>();
        if (hbi.isPartOfMultipart)
        {
            foreach (Transform i in hb.transform.parent) // add player to all hitboxes in folder
            {
                if (i.TryGetComponent(out HitboxInfo ihbi))
                {
                    ihbi.playersHitAlready.Add(player);
                }
            }
        }
        else
        {
            hbi.playersHitAlready.Add(player);
        }
    }

    List<Transform> GetConnectedHitboxes(GameObject hb)
    {
        List<Transform> hitboxesToRemove = new List<Transform>();

        if (hb.TryGetComponent(out HitboxInfo hbi))
        {
            if (hbi.isPartOfMultipart)
            {
                foreach (Transform i in hb.transform.parent) // disable all hitboxes in folder
                {
                    hitboxesToRemove.Add(i);
                }
            }
            else
            {
                hitboxesToRemove.Add(hb.transform);
            }
        }
        else
        {
            Debug.LogWarning("Called GetConnectedHitboxes on an object that is not a hitbox. That's not good.");
        }

        return hitboxesToRemove;
    }

    void DisableHitboxes(GameObject partOfMultipart)
    {
        foreach (Transform hitbox in GetConnectedHitboxes(partOfMultipart))
        {
            hitbox.gameObject.SetActive(false);
            if (hitbox.TryGetComponent(out HitboxInfo hbi))
            {
                hbi.doNotEnable = true; // if doNotEnable is true, collisions with players will be ignored, and will not be enabled later in the same move
                doNotEnableHitboxes.Add(hitbox.gameObject);
            }
        }
    }

    public IEnumerator AfterPhysicsUpdate() // Using WaitForFixedUpdate is the only way to run something AFTER the Collision calls
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            RunHitBoxManagement();
            RunGrabManagement();

            hitboxPlayerCol.Clear();
            hitboxHitboxCol.Clear();
            hitboxCounterCol.Clear();
            grabPlayerCol.Clear();
        }

        // Runs through every collider that has collided this frame, and adds them to either hitboxes or hurtboxes
        // Removes inactive hitboxes and deactives touching hitboxes.
        // Finally checks if hurtboxes are touching hitboxes

        void RunHitBoxManagement()
        {
            if (hitboxPlayerCol.Count + hitboxHitboxCol.Count + hitboxCounterCol.Count + grabPlayerCol.Count == 0) { return; }

            // Hitboxes collided with each other this frame. Dealing with hitbox on hitbox interactions:
            if (hitboxHitboxCol.Count > 0)
            {
                foreach (Pair<Collider2D, Collider2D> hitboxPair in hitboxHitboxCol) // iterate the list of all hitbox on hitbox combinations
                {
                    HitboxInfo leftHBI = hitboxPair.left.GetComponent<HitboxInfo>();
                    HitboxInfo rightHBI = hitboxPair.right.GetComponent<HitboxInfo>();

                    if (leftHBI.isDestroyable || rightHBI.isDestroyable)
                    {
                        if (leftHBI.isDestroyable)
                        {
                            leftHBI.objectHealth -= rightHBI.damage;
                            if (leftHBI.objectHealth <= 0)
                            {
                                SelfDestructObject?.Invoke(leftHBI.gameObject);
                            }
                        }
                        else
                        {
                            rightHBI.objectHealth -= leftHBI.damage;
                            if (rightHBI.objectHealth <= 0)
                            {
                                SelfDestructObject?.Invoke(rightHBI.gameObject);
                            }
                        }
                    }
                    else if (leftHBI.owner != rightHBI.owner && !rightHBI.cannotClank && !leftHBI.cannotClank)
                    {
                        if (Mathf.Abs(leftHBI.damage - rightHBI.damage) > hitboxDamageDifferenceToWin) // if damage diff > diff to beat out
                        {
                            if (leftHBI.damage > rightHBI.damage) // disable the weaker hitbox, because it must be much weaker
                            {
                                DisableHitboxes(hitboxPair.left.gameObject);
                            }
                            else
                            {
                                DisableHitboxes(hitboxPair.right.gameObject);
                            }
                        }
                        else // hitboxes are similar damages, disable both
                        {
                            em.SpawnHitEffectOnContactPoint("ClankEffect1", hitboxPair.left, hitboxPair.right.bounds.center);

                            DisableHitboxes(hitboxPair.left.gameObject);
                            DisableHitboxes(hitboxPair.right.gameObject);
                        }
                    }
                }
            }

            if (hitboxCounterCol.Count > 0) // registering counter hits before the hitboxes are registered hitting anything
            {
                foreach (Pair<Collider2D, Collider2D> counterHitboxPair in hitboxCounterCol)
                {
                    HitboxInfo hbi = counterHitboxPair.left.GetComponent<HitboxInfo>();
                    GameObject counterOwner = counterHitboxPair.right.GetComponent<HitboxInfo>().owner;

                    // if counter is touching hitbox, and the hitbox doesn't belong to the same player as the counter, and that player hasn't already been hit by the counter
                    if (hbi.owner != counterOwner && !counterHitboxPair.right.GetComponent<HitboxInfo>().playersHitAlready.Contains(hbi.owner))
                    {
                        DisableHitboxes(counterHitboxPair.left.gameObject);
                        hbi.owner.GetComponent<Control1>().Hit(counterHitboxPair.right); // this means counters always hit the player, even with projectiles
                    }
                }
            }


            // order by priority and deal damage and knockback to players hit by hitboxes
            hitboxPlayerCol = hitboxPlayerCol.OrderBy(pair => pair.left.GetComponent<HitboxInfo>().priority).ToList();
            foreach (Pair<Collider2D, Collider2D> i in hitboxPlayerCol) // hitbox left, hurtbox right
            {
                HitboxInfo hbi = i.left.GetComponent<HitboxInfo>();
                if (!hbi.playersHitAlready.Contains(i.right.gameObject) && !hbi.doNotEnable) // check if the hitbox has had the player added during the move already OR deactivated
                {
                    Control1 hurtboxC1 = i.right.GetComponent<Control1>();
                    if (hurtboxC1.intangible)
                    {
                        continue;
                    }

                    Control1 hitboxC1 = hbi.owner.GetComponent<Control1>();
                    hurtboxC1.FreezeFrames(0, hbi.hitstopFrames); // apply hitstop defending player
                    if (!hbi.isProjectile) // is not projectile; apply hitstop attacking player too; if projectile the attacking player is not frozen
                    {
                        hitboxC1.FreezeFrames(0, hbi.hitstopFrames);
                    }

                    if (!string.IsNullOrEmpty(hbi.hitsound)) // play hit sound
                    {
                        if (hbi.soundGroupInsteadOfSound)
                        {
                            hurtboxC1.am.PlaySoundGroup(hbi.hitsound);
                        }
                        else
                        {
                            hurtboxC1.am.PlaySound(hbi.hitsound);
                        }
                    }

                    hurtboxC1.Hit(i.left.GetComponent<Collider2D>(), true); // apply knockback to defending player

                    em.SpawnHitEffectOnContactPoint(GetApproriateEffect(hbi.damage), i.left.GetComponent<Collider2D>(), i.right.transform.position);
                    AddPlayerToConnectedHitboxes(i.left.gameObject, i.right.gameObject); // add hit player to connected hitboxes so they cannot also hit them

                    SelfDestructObject?.Invoke(i.left.gameObject);
                }
            }
        }

        void RunGrabManagement()
        {
            foreach (Pair<Collider2D, Collider2D> i in grabPlayerCol) // left grab hitbox, right player hitbox
            {
                if (i.left.GetComponent<HitboxInfo>().owner != i.right.gameObject)
                {
                    i.right.transform.root.GetComponent<Control1>().Grabbed(i.left);
                    DeathHeavyClawControl dhcc = i.left.transform.root.GetComponent<DeathHeavyClawControl>();
                    dhcc.grabbedPlayer = i.right.gameObject;
                    dhcc.grabbedPlayerRB = i.right.GetComponent<Rigidbody2D>();
                    dhcc.grabbedPlayerRB.velocity = Vector2.zero;

                    em.SpawnHitEffectOnContactPoint(GetApproriateEffect(i.left.GetComponent<HitboxInfo>().damage), i.left.GetComponent<Collider2D>(), i.right.transform.position);
                }
            }
        }
    }

    public string GetApproriateEffect(float damage)
    {
        if (damage < 5)
        {
            return "MinorHitEffect";
        }
        else if (damage >= 5 && damage < 10)
        {
            return "MediumHitEffect1";
        }
        else
        {
            return "MajorHitEffect1";
        }

    }
}
