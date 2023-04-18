using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering.Universal;

public class StanleyPaperControl : MonoBehaviour
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
            Destroy(gameObject);
            him.SelfDestructObject -= SelfDestruct;
        }
    }
}
