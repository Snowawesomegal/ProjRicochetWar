using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathDairHChainController : MonoBehaviour
{
    public int goalPositionCount;
    LineRenderer lr;
    public DeathAnimationEvents ownerDAE;
    int setPositions = 1;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (setPositions < goalPositionCount)
        {
            lr.positionCount = lr.positionCount + 1 ;
            lr.SetPosition(setPositions, lr.GetPosition(setPositions - 1) - new Vector3(0, 2, 0));
            setPositions += 1;
        }
        else
        {
            ownerDAE.ChainHitTheGround();
            this.enabled = false;
        }

        if (ownerDAE.c1.clm.activeLockers.Contains(ownerDAE.c1.hitstun))
        {
            Destroy(gameObject);
        }
    }
}
