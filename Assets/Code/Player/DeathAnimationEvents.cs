using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationEvents;

public class DeathAnimationEvents : MonoBehaviour
{
    public GameObject heavyClaw;
    Control1 c1;
    public bool upwardHeavyClawExists = false;


    private void Start()
    {
        c1 = GetComponent<Control1>();
    }

    public void SpawnHeavyClaw()
    {
        if (!upwardHeavyClawExists)
        {
            GameObject newHeavyClaw = Instantiate(heavyClaw, transform.position + new Vector3(1.7f * (c1.facingRight ? 1 : -1), -1, 0), Quaternion.Euler(AngleMath.Vector2FromAngle(0)));
            newHeavyClaw.GetComponent<DeathHeavyClawControl>().owner = gameObject;
            upwardHeavyClawExists = true;
        }
    }
}
