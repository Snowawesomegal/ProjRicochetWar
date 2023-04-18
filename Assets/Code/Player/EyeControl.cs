using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EyeControl : MonoBehaviour
{
    List<GameObject> players;
    GameObject currentTarget;
    Control1 targetControl1;
    int chooseTargetCountdown = 60;
    int currentTargetIndex = 0;
    GameObject hitbox;

    public GameObject destroyedPS;

    int framesLifetime = 360;
    int framesActive = 0;

    public GameObject reticlePrefab;
    GameObject reticle;
    public GameObject owner;
    Animator anim;
    int openCountdown = 60;
    bool open = false;
    HitboxInteractionManager him;

    // Start is called before the first frame update
    void Start()
    {
        him = Camera.main.GetComponent<HitboxInteractionManager>();
        anim = GetComponent<Animator>();
        transform.GetChild(0).GetComponent<HitboxInfo>().owner = owner;
        players = GameObject.FindGameObjectsWithTag("Player").ToList();
        players.Remove(owner);

        if (players.Count == 1) // theres only one player, so skip all the targeting, select our player and set up
        {
            chooseTargetCountdown = -1;
            currentTarget = players[0];
            targetControl1 = currentTarget.GetComponent<Control1>();
        }
        else if (players.Count > 1)
        {
            currentTarget = GetClosest(players);
        }

        if (currentTarget != null)
        {
            reticle = Instantiate(reticlePrefab, currentTarget.transform.position, Quaternion.identity);
        }

        him.SelfDestructObject += gameObj => SelfDestruct(gameObj);
        hitbox = transform.GetChild(0).gameObject;
    }

    private void FixedUpdate()
    {
        if (chooseTargetCountdown > 0) // if countdown is not over, update ordered player list
        {
            players.OrderBy(player => player.transform.position.x); // order players left to right, so you can move the reticle horizontally easily

            if (currentTarget != null)
            {
                currentTargetIndex = players.FindIndex(a => a.gameObject == gameObject); // update index to where the player is now
            }
            else
            {
                currentTarget = GetClosest(players); // if there is no target (because they died etc) snap to nearest player
            }

            // move reticle to target position

            chooseTargetCountdown -= 1;
        }
        else if (chooseTargetCountdown == 0) // countdown is over; set target
        {
            if (targetControl1 != null)
            {
                targetControl1 = currentTarget.GetComponent<Control1>();
            }
            chooseTargetCountdown -= 1;
        }

        if (reticle != null)
        {
            MoveReticle();
        }

        if (open)
        {
            if (openCountdown > 0)
            {
                openCountdown -= 1;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, 0.1f);
                Vector2 between = (currentTarget.transform.position - transform.position).normalized;
                anim.SetFloat("XDif", between.x);
                anim.SetFloat("YDif", between.y);

                framesActive += 1;
                if (framesActive >= framesLifetime)
                {
                    SelfDestruct(hitbox);
                }
            }
        }
        else
        {
            if (targetControl1 != null)
            {
                if (targetControl1.firstFrameOfHitstun)
                {
                    OpenEye();
                }
            }
        }
    }

    GameObject GetClosest(List<GameObject> objs)
    {
        Pair<float, GameObject> closest = new Pair<float, GameObject>(666, null); // distance, object
        foreach (GameObject i in objs)
        {
            float distance = Vector3.Distance(i.transform.position, transform.position);
            if (distance < closest.left)
            {
                closest = new Pair<float, GameObject>(distance, i);
            }
        }
        return closest.right;
    }

    void MoveReticle()
    {
        if (reticle != null)
        {
            reticle.transform.position = currentTarget.transform.position;
        }
    }

    public void ChangeCurrentTarget(bool rightleft) // to be called by player. somehow, idk
    {
        if (chooseTargetCountdown > 0)
        {
            int intendedIndex = currentTargetIndex + (rightleft ? 1 : -1);

            if (intendedIndex > players.Count - 1) // wrap back around if you went too high
            {
                intendedIndex = 0;
            }
            if (intendedIndex < players.Count - 1)
            {
                intendedIndex = players.Count - 1;
            }
            currentTarget = players[intendedIndex]; // move reticle to a different player

            MoveReticle(); // update reticle position
        }
    }

    public void OpenEye()
    {
        Debug.Log("open");
        openCountdown = 60;
        open = true;
        anim.SetBool("Open", true);
    }
    
    public void SelfDestruct(GameObject whatObjectIsDestroyed)
    {
        if (whatObjectIsDestroyed == hitbox)
        {
            Destroy(Instantiate(destroyedPS, transform.position, Quaternion.identity), 3);
            Destroy(reticle);
            Destroy(gameObject);
            him.SelfDestructObject -= SelfDestruct;
            owner.GetComponent<DeathAnimationEvents>().eyeExists = false;
        }
    }
}
