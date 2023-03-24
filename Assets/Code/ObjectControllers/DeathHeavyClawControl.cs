using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHeavyClawControl : MonoBehaviour
{
    public GameObject owner = null;

    public void EnableGrabHitbox() // to be called by animation event
    {
        // enable grab hitbox
    }

    public void DisableGrabHitbox() // to be called by animation event
    {
        // disable grab hitbox
    }

    private void OnTriggerEnter2D(Collider2D collision) // if player touching grab hitbox is not owner, grab player
    {
        if (collision.gameObject != owner)
        {
            if (collision.TryGetComponent(out Control1 c1))
            {
                // grab player
            }
        }

    }
}
