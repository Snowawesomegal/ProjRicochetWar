using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyMonitorControl : MonoBehaviour
{
    HitboxInteractionManager him;
    GameObject hitbox;

    void Start()
    {
        hitbox = transform.GetChild(0).gameObject;

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        him.SelfDestructObject += gameObj => SelfDestruct(gameObj);
    }

    public void SelfDestruct(GameObject whatObjectIsDestroyed)
    {
        if (whatObjectIsDestroyed == hitbox)
        {
            hitbox.GetComponent<HitboxInfo>().owner.GetComponent<StanleyAnimationEvents>().activeMonitors -= 1;

            Destroy(gameObject);
            him.SelfDestructObject -= SelfDestruct;
        }
    }

    public void ManualSelfDestruct()
    {
        SelfDestruct(hitbox);
    }
}
