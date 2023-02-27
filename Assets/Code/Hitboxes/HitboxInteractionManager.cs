using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxInteractionManager : MonoBehaviour
{
    public List<Collider2D> triggersThisFrame = new List<Collider2D>();
    public float hitboxDamageDifferenceToWin = 10;
    ActivateHitbox ah;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(AfterPhysicsUpdate));
        ah = GetComponent<ActivateHitbox>();
    }

    void DisableConnectedHitboxes(GameObject hbi)
    {
        if (ah.currentConnectedHitboxes.Contains(hbi))
        {
            foreach (GameObject i in ah.currentConnectedHitboxes)
            {
                i.GetComponent<HitboxInfo>().activeHitbox = false;
            }
        }

    }

    public IEnumerator AfterPhysicsUpdate() // Using WaitForFixedUpdate is the only way to run something AFTER the Collision calls
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            foreach (Collider2D i in triggersThisFrame) { Debug.Log(i); }
            RunHitBoxManagement();
            triggersThisFrame.Clear();
        }


        // Runs through every collider that has collided this frame, and adds them to either hitboxes or hurtboxes
        // Removes inactive hitboxes and deactives touching hitboxes.
        // Finally checks if hurtboxes are touching hitboxes

        void RunHitBoxManagement()
        {
            if (triggersThisFrame.Count == 0) { return; }

            List<Collider2D> hitboxes = new List<Collider2D>();
            List<Collider2D> hurtboxes = new List<Collider2D>();

            foreach(Collider2D i in triggersThisFrame) // Get separate lists of hitboxes and hurtboxes, exclude walls
            {
                if (i.TryGetComponent(out HitboxInfo hbi))
                {
                    hitboxes.Add(i);
                    Debug.Log("Detected hitbox collision, added the hitbox collision to hitboxes");
                }
                else if (i.TryGetComponent(out Control1 c1)) // THIS WILL BREAK IF WE USE A DIFFERENT SCRIPT FOR CONTROL (or Hit()) //TODO
                {
                    hurtboxes.Add(i);
                    Debug.Log("Added hurtbox to hurtboxes");
                }
            }

            List<Collider2D> hitboxesCopy = new List<Collider2D>(hitboxes);

            // Don't open this lmao it has 50 lines of logic, removes and deactives hitboxes as necessary
            if (hitboxes.Count == 1)
            {
                if (!hitboxes[0].GetComponent<HitboxInfo>().activeHitbox)
                {
                    hitboxes.Clear();
                }
            }
            else
            {
                foreach (Collider2D one in hitboxesCopy) // run through every hitbox that collided this frame
                {
                    foreach (Collider2D two in hitboxesCopy) // and every other hitbox that collided this frame
                    {
                        if (one == two) // ignore if same hitbox
                        {
                            continue;
                        }

                        HitboxInfo oneHitboxInfo = one.GetComponent<HitboxInfo>();
                        HitboxInfo twoHitboxInfo = two.GetComponent<HitboxInfo>();

                        bool contBool = false;
                        if (!oneHitboxInfo.activeHitbox) // if either is inactive already, remove from list, then skip next
                        {
                            hitboxes.Remove(one);
                            contBool = true;
                        }
                        if (!twoHitboxInfo.activeHitbox)
                        {
                            hitboxes.Remove(two);
                            contBool = true;
                        }
                        if (contBool) { continue; }

                        if (one.IsTouching(two) && oneHitboxInfo.activeHitbox && twoHitboxInfo.activeHitbox) // if hitboxes are colliding + both active
                        {
                            if (Mathf.Abs(oneHitboxInfo.damage - twoHitboxInfo.damage) > hitboxDamageDifferenceToWin) // if damage diff > diff to win
                            {
                                if (oneHitboxInfo.damage > twoHitboxInfo.damage) // disable the weaker hitbox, because it must be much weaker
                                {
                                    oneHitboxInfo.activeHitbox = false;
                                    hitboxes.Remove(one);
                                    DisableConnectedHitboxes(one.gameObject);
                                }
                                else
                                {
                                    twoHitboxInfo.activeHitbox = false;
                                    hitboxes.Remove(two);
                                    DisableConnectedHitboxes(two.gameObject);
                                }
                            }
                            else // hitboxes are similar damages, disable both
                            {
                                DisableConnectedHitboxes(one.gameObject);
                                DisableConnectedHitboxes(two.gameObject);
                                oneHitboxInfo.activeHitbox = false;
                                twoHitboxInfo.activeHitbox = false;
                                hitboxes.Remove(one); // remove hitboxes from list
                                hitboxes.Remove(two);
                            }
                        }
                    }
                }
            }


            foreach(Collider2D hurtbox in hurtboxes)
            {
                foreach (Collider2D hitbox in hitboxes)
                {
                    if (hurtbox.IsTouching(hitbox) && hitbox.GetComponent<HitboxInfo>().owner != hurtbox.gameObject)
                    {
                        Debug.Log("Hurtbox: " + hurtbox.name + " was touching: " + hitbox.name);

                        hurtbox.GetComponent<Control1>().Hit(hitbox, true);
                        hitbox.GetComponent<HitboxInfo>().activeHitbox = false;
                    }
                }
            }
        }

    }
}
