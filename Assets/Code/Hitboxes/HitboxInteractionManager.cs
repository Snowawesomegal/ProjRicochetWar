using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HitboxInteractionManager : MonoBehaviour
{
    public List<Collider2D> triggersThisFrame = new List<Collider2D>();
    public float hitboxDamageDifferenceToWin = 10;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(AfterPhysicsUpdate));
    }

    void AddPlayerToConnectedHitboxes(GameObject hb, GameObject player)
    {
        HitboxInfo hbi = hb.GetComponent<HitboxInfo>();
        if (hbi.isPartOfMultipart)
        {
            foreach (Transform i in hb.transform.parent) // add player to all hitboxes in folder
            {
                i.GetComponent<HitboxInfo>().playersHitAlready.Add(player);
            }
        }
        else
        {
            hbi.playersHitAlready.Add(player);
        }
    }

    void DisableConnectedHitboxes(GameObject hb)
    {
        HitboxInfo hbi = hb.GetComponent<HitboxInfo>();
        if (hbi.isPartOfMultipart)
        {
            foreach (Transform i in hb.transform.parent) // disable all hitboxes in folder
            {
                i.gameObject.SetActive(false);
            }
        }
        else
        {
            hb.SetActive(false);
        }
    }

    public IEnumerator AfterPhysicsUpdate() // Using WaitForFixedUpdate is the only way to run something AFTER the Collision calls
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

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
                }
                else if (i.TryGetComponent(out Control1 c1)) // THIS WILL BREAK IF WE USE A DIFFERENT SCRIPT FOR CONTROL (or Hit()) //TODO
                {
                    hurtboxes.Add(i);
                }
            }

            List<Collider2D> hitboxesCopy = new List<Collider2D>(hitboxes);

            if (hitboxes.Count > 1)// more than one hitbox collided with something this frame. Dealing with collider interactions:
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

                    if (one.IsTouching(two))
                    {
                        if (Mathf.Abs(oneHitboxInfo.damage - twoHitboxInfo.damage) > hitboxDamageDifferenceToWin) // if damage diff > diff to beat out
                        {
                            if (oneHitboxInfo.damage > twoHitboxInfo.damage) // disable the weaker hitbox, because it must be much weaker
                            {
                                one.gameObject.SetActive(false);
                                hitboxes.Remove(one);
                                DisableConnectedHitboxes(one.gameObject);
                            }
                            else
                            {
                                two.gameObject.SetActive(false);
                                hitboxes.Remove(two);
                                DisableConnectedHitboxes(two.gameObject);
                            }
                        }
                        else // hitboxes are similar damages, disable both
                        {
                            DisableConnectedHitboxes(one.gameObject);
                            DisableConnectedHitboxes(two.gameObject);
                            one.gameObject.SetActive(false);
                            two.gameObject.SetActive(false);
                            hitboxes.Remove(one); // remove hitboxes from list
                            hitboxes.Remove(two);
                        }
                    }
                }
            }

            List<Pair<GameObject, GameObject>> hitlist = new List<Pair<GameObject, GameObject>>(); // stores all hurtbox/hitbox collisions, Hitbox is Left, Hurtbox is Right
            foreach(Collider2D hurtbox in hurtboxes)
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
                if (!i.left.GetComponent<HitboxInfo>().playersHitAlready.Contains(i.right)) // check if the hitbox has had the player added recentlu
                {
                    Debug.Log(i.right.name + " was hit by " + i.left.name);
                    i.right.GetComponent<Control1>().Hit(i.left.GetComponent<Collider2D>(), true);
                    AddPlayerToConnectedHitboxes(i.left, i.right); // add hit player to connected hitboxes so they cannot also hit them
                }
            }
        }
    }
}
