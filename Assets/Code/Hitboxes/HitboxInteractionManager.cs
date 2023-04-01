using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HitboxInteractionManager : MonoBehaviour
{
    EffectManager em;

    public List<Collider2D> triggersThisFrame = new List<Collider2D>();
    public List<Pair<Collider2D, Collider2D>> grabboxesAndPlayersThisFrame = new List<Pair<Collider2D, Collider2D>>();

    public List<GameObject> doNotEnableHitboxes = new List<GameObject>();

    public float hitboxDamageDifferenceToWin = 10;

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
            Debug.Log("You just tried to involve a pogobox with a collision or something, code is very broken");
        }

        return hitboxesToRemove;
    }

    public IEnumerator AfterPhysicsUpdate() // Using WaitForFixedUpdate is the only way to run something AFTER the Collision calls
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            RunHitBoxManagement();
            RunGrabManagement();
            triggersThisFrame.Clear();
        }

        // Runs through every collider that has collided this frame, and adds them to either hitboxes or hurtboxes
        // Removes inactive hitboxes and deactives touching hitboxes.
        // Finally checks if hurtboxes are touching hitboxes

        void RunHitBoxManagement()
        {
            if (triggersThisFrame.Count == 0) { return; }

            List<Collider2D> hitboxes = new List<Collider2D>();
            List<Collider2D> playerHurtboxes = new List<Collider2D>();
            List<Collider2D> counterBoxes = new List<Collider2D>();

            foreach(Collider2D i in triggersThisFrame) // Get separate lists of hitboxes and hurtboxes, exclude walls
            {
                if (i.TryGetComponent(out HitboxInfo hbi))
                {
                    if (hbi.isCounter)
                    {
                        counterBoxes.Add(i);
                    }
                    else if (hbi.isGrab)
                    {
                        // nothing
                    }
                    else
                    {
                        hitboxes.Add(i);
                    }

                }
                else if (i.TryGetComponent(out Control1 c1)) // THIS WILL BREAK IF WE USE A DIFFERENT SCRIPT FOR CONTROL (or Hit()) //TODO
                {
                    playerHurtboxes.Add(i);
                }
            }

            void DisableAndRemoveHitboxes(GameObject baseBox)
            {
                foreach (Transform hitbox in GetConnectedHitboxes(baseBox))
                {
                    hitbox.gameObject.SetActive(false);
                    hitboxes.Remove(hitbox.GetComponent<Collider2D>());
                    if (hitbox.TryGetComponent(out HitboxInfo hbi2))
                    {
                        hbi2.doNotEnable = true;
                        doNotEnableHitboxes.Add(hitbox.gameObject);
                    }
                }
            }

            List<Collider2D> hitboxesCopy = new List<Collider2D>(hitboxes);

            // more than one hitbox collided with something this frame. Dealing with hitbox on hitbox interactions:
            if (hitboxes.Count > 1)
            {
                List<List<Collider2D>> allCombinations = new List<List<Collider2D>>(); // List of all combinations of Colliders that collided this frame + are not of the same parent
                List<Collider2D> newCollider = new List<Collider2D>();

                foreach (Collider2D one in hitboxesCopy) // run through every hitbox that collided this frame
                {
                    foreach (Collider2D two in hitboxesCopy) // and other hitbox that collided this frame again
                    {
                        if (one == two || one.gameObject == two.gameObject) // if colliders are not the same or from the same parent
                        {
                            continue;
                        }

                        newCollider = new List<Collider2D>() { one, two };
                        if (!allCombinations.Contains(newCollider)) // add them to a list if they form a unique combination
                        {
                            allCombinations.Add(newCollider);
                        }
                    }
                }

                foreach (List<Collider2D> i in allCombinations) // iterate the list of all unique collider combinations
                {
                    Collider2D one = i[0];
                    Collider2D two = i[1];

                    HitboxInfo oneHitboxInfo = one.GetComponent<HitboxInfo>();
                    HitboxInfo twoHitboxInfo = two.GetComponent<HitboxInfo>();

                    if (one.IsTouching(two) && oneHitboxInfo.owner != twoHitboxInfo.owner)
                    {
                        if (Mathf.Abs(oneHitboxInfo.damage - twoHitboxInfo.damage) > hitboxDamageDifferenceToWin) // if damage diff > diff to beat out
                        {
                            if (oneHitboxInfo.damage > twoHitboxInfo.damage) // disable the weaker hitbox, because it must be much weaker
                            {
                                DisableAndRemoveHitboxes(one.gameObject);
                            }
                            else
                            {
                                DisableAndRemoveHitboxes(two.gameObject);
                            }
                        }
                        else // hitboxes are similar damages, disable both
                        {
                            em.SpawnHitEffectOnContactPoint("ClankEffect1", one, two.bounds.center);

                            DisableAndRemoveHitboxes(one.gameObject);
                            DisableAndRemoveHitboxes(two.gameObject);
                        }
                    }
                }
            }

            if (counterBoxes.Count > 0) // registering counter hits before the hitboxes are registered hitting anything
            {
                foreach (Collider2D counter in counterBoxes)
                {
                    Debug.Log("a counterbox was active");
                    List<Collider2D> hitboxesCopy2 = new List<Collider2D>(hitboxes);
                    foreach (Collider2D hitbox in hitboxesCopy2)
                    {
                        HitboxInfo hbi = hitbox.GetComponent<HitboxInfo>();
                        GameObject counterOwner = counter.GetComponent<HitboxInfo>().owner;

                        // if counter is touching hitbox, and the hitbox doesn't belong to the same player as the counter, and that player hasn't already been hit by the counter
                        if (counter.IsTouching(hitbox) && hbi.owner != counterOwner && !counter.GetComponent<HitboxInfo>().playersHitAlready.Contains(hbi.owner))
                        {
                            DisableAndRemoveHitboxes(hitbox.gameObject);
                            hbi.owner.GetComponent<Control1>().Hit(counter); // this means counters always hit the player, even with projectiles
                        }
                    }
                }
            }

            List<Pair<GameObject, GameObject>> hitlist = new List<Pair<GameObject, GameObject>>(); // stores all hurtbox/hitbox collision pairs, Hitbox is Left, Hurtbox is Right
            foreach(Collider2D hurtbox in playerHurtboxes)
            {
                foreach (Collider2D hitbox in hitboxes)
                {
                    HitboxInfo hbi = hitbox.GetComponent<HitboxInfo>();
                    GameObject hurtboxGameObject = hurtbox.gameObject;

                    // if player is touching hitbox, and the hitbox doesn't belong to that player, and that player hasn't already been hit by that hitbox
                    if (hurtbox.IsTouching(hitbox) && hbi.owner != hurtboxGameObject && !hbi.playersHitAlready.Contains(hurtboxGameObject))
                    {
                        hitlist.Add(new Pair<GameObject, GameObject>(hitbox.gameObject, hurtbox.gameObject));
                    }
                }
            }

            // order by priority and FINALLY deal damage and knockback
            hitlist = hitlist.OrderBy(pair => pair.left.GetComponent<HitboxInfo>().priority).ToList();
            foreach (Pair<GameObject, GameObject> i in hitlist) // hitbox left, hurtbox right
            {
                HitboxInfo hbi = i.left.GetComponent<HitboxInfo>();
                if (!hbi.playersHitAlready.Contains(i.right)) // check if the hitbox has had the player added during the move already
                {
                    Control1 hurtboxC1 = i.right.GetComponent<Control1>();
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
                    AddPlayerToConnectedHitboxes(i.left, i.right); // add hit player to connected hitboxes so they cannot also hit them

                    if (hbi.transform.root.TryGetComponent(out EyeControl eyeControl)) // this is a really stupid way to handle this one case, I should probably fix this
                    {
                        eyeControl.SelfDestruct();
                    }
                }
            }
        }

        void RunGrabManagement()
        {
            foreach (Pair<Collider2D, Collider2D> i in grabboxesAndPlayersThisFrame) // left grab hitbox, right player hitbox
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
            grabboxesAndPlayersThisFrame.Clear();
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
        else if (damage >= 10)
        {
            return "MajorHitEffect1";
        }
        else
        {
            return "MediumHitEffect1";
        }
    }
}
