using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationEvents;

public class DeathAnimationEvents : MonoBehaviour
{
    public GameObject heavyClaw;
    Control1 c1;
    public bool upwardHeavyClawExists = false;
    public bool horizontalHeavyClawExists = false;


    private void Start()
    {
        c1 = GetComponent<Control1>();
    }

    public void SpawnHeavyClaw()
    {
        if (!upwardHeavyClawExists)
        {
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(1.7f * (c1.facingRight ? 1 : -1), -1, 0),
                Quaternion.Euler(0f, 0f + (c1.facingRight?180:0), 0f));
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            upwardHeavyClawExists = true;
        }
    }

    public void SpawnHorizontalHeavyClaw()
    {
        if (!horizontalHeavyClawExists)
        {
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(-1.5f * (c1.facingRight ? 1 : -1), 0.1f, 0),
                Quaternion.Euler(0f + (c1.facingRight ? 0 : 180), 0, 90 * (c1.facingRight ? -1 : 1)));
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().horizontal = true;
            horizontalHeavyClawExists = true;

        }
    }
}
