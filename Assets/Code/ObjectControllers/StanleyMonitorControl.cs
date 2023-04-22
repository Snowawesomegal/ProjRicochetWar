using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyMonitorControl : MonoBehaviour
{
    HitboxInteractionManager him;
    GameObject hitbox;
    HitboxInfo hbi;

    void Start()
    {
        hitbox = transform.GetChild(0).gameObject;

        him = Camera.main.GetComponent<HitboxInteractionManager>();

        him.SelfDestructObject += gameObj => SelfDestruct(gameObj);

        hbi = hitbox.GetComponent<HitboxInfo>();
    }

    public void SelfDestruct(GameObject whatObjectIsDestroyed)
    {
        if (whatObjectIsDestroyed == hitbox)
        {
            hbi.owner.GetComponent<StanleyAnimationEvents>().activeMonitors -= 1;

            hbi.owner.GetComponent<Control1>().em.SpawnDirectionalEffect("Explosion1", transform.position, hbi.facingRight);

            him.SelfDestructObject -= SelfDestruct;

            Destroy(gameObject);
        }
    }

    public void ManualSelfDestruct()
    {
        SelfDestruct(hitbox);
    }
}
