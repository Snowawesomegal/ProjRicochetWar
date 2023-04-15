using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeathDairHChainController : MonoBehaviour
{
    // This object is a temporary object; any StopEverything calls by its owner will destroy it.

    public int goalPositionCount;
    LineRenderer lr;
    public DeathAnimationEvents ownerDAE;
    int setPositions = 1;
    CapsuleCollider2D hitbox;
    bool hitTheGround = false;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        hitbox = transform.GetChild(0).GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (setPositions < goalPositionCount) // if chain still extending
        {
            lr.positionCount++;
            lr.SetPosition(setPositions, lr.GetPosition(setPositions - 1) - new Vector3(0, 2, 0));
            setPositions += 1;

            hitbox.offset = new Vector2(0, -goalPositionCount);
            hitbox.size = new Vector2(1f, goalPositionCount * 2);
        }
        else
        {
            if (hitTheGround) // chain already hit the ground but Death hasn't yet
            {
                Vector3[] temp = new Vector3[lr.positionCount];
                lr.GetPositions(temp);
                List<Vector3> allPositions = temp.ToList();

                if (ownerDAE.transform.position.y < temp[1].y) // if player lower than second position, remove first position
                {
                    allPositions.RemoveAt(0);
                    lr.positionCount = allPositions.Count();
                    lr.SetPositions(allPositions.ToArray());
                }
            }
            else // this is the first frame of the chain being on the ground
            {
                ownerDAE.ChainHitTheGround();
                hitTheGround = true;
                hitbox.gameObject.SetActive(false);
            }
        }
    }
}
