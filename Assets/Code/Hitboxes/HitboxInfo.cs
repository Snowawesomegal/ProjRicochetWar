using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxInfo : MonoBehaviour
{
    public float damage = 10;
    public float knockback = 100;
    public bool angleIndependentOfMovement = true;
    public float angle = 45;
    public GameObject owner;
    public bool facingRight = true;
    public int activeFrames = 2;

    public bool activeHitbox = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out HitboxInfo _) || collision.TryGetComponent(out Control1 _))
        {
            HitboxInteractionManager him = Camera.main.GetComponent<HitboxInteractionManager>();
            Collider2D cd = GetComponent<Collider2D>();

            if (!him.triggersThisFrame.Contains(collision))
            {
                him.triggersThisFrame.Add(collision);
            }
            if (!him.triggersThisFrame.Contains(collision))
            {
                him.triggersThisFrame.Add(cd);
            }
        }
    }


}
