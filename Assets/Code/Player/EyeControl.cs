using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EyeControl : MonoBehaviour
{
    List<GameObject> players;
    GameObject currentTarget;
    GameObject target;
    Control1 targetControl1;
    int chooseTargetCountdown = 60;
    int currentTargetIndex = 0;
    public GameObject reticlePrefab;
    GameObject reticle;
    public GameObject owner;
    Animator anim;
    int openCountdown = 60;
    bool open = false;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player").ToList();
        players.Remove(owner);

        if (players.Count == 1)
        {
            chooseTargetCountdown = 0;
            target = currentTarget;
            reticle = Instantiate(reticlePrefab, currentTarget.transform.position, Quaternion.identity);
        }
        else if (players.Count > 1)
        {
            currentTarget = GetClosest(players);
            reticle = Instantiate(reticlePrefab, currentTarget.transform.position, Quaternion.identity);
        }
    }

    private void FixedUpdate()
    {
        if (chooseTargetCountdown > 0)
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

            MoveReticle(); // move reticle to target position

            chooseTargetCountdown -= 1;
        }
        else if (chooseTargetCountdown == 0) // countdown is over; set target
        {
            target = currentTarget;
            chooseTargetCountdown -= 1;
            Destroy(reticle);
        }

        if (open)
        {
            if (openCountdown > 0)
            {
                openCountdown -= 1;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, 0.1f);
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

    void ChangeCurrentTarget(bool rightleft) // to be called by player. somehow, idk
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
        openCountdown = 60;
        open = true;
        anim.SetBool("Open", true);
    }
}
