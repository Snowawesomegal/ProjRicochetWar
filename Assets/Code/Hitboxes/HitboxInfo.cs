using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxInfo : MonoBehaviour
{
    public float damage = 10;
    public float knockback = 100;
    public bool angleIndependentOfMovement = true;

    [Tooltip("361 moves opponent to right in front of player, 362 moves opponent toward right above player")]
    public float angle = 45;
    public GameObject owner;
    public bool facingRight = true;
    public int activeFrames = 2;
    public int minimumHitstunFrames = 20;

    [Tooltip("Set true in all connected hitboxes when a hitbox clanks so that the rest of the hitboxes activated afterward in the same move are not activated.")]
    public bool doNotEnable = false;

    [Tooltip("LOWER number priority wins; lower priority hitboxes are disabled if two hitboxes from the same move hit a player simultaneously.")]
    public int priority = 1;

    public List<GameObject> playersHitAlready;

    [Tooltip("Set this to true if the hitbox has multiple hitboxes that are part of the same move as it, and are disabled if it connects. In this case, this object should also be parented to a another gameobject. See Ghost UpHeavy. This is checked by HitboxInteractionManager.")]
    public bool isPartOfMultipart = false;

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
