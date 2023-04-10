using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class HitboxInfo : MonoBehaviour
{
    public float damage = 10;
    public float kbSpeedMultiplier = 1;
    public float kbDistance = 8;

    public string hitsound = null;
    public bool soundGroupInsteadOfSound = false;

    [Tooltip("361 moves opponent to right in front of player, 362 moves opponent toward right above player")]
    public float angle = 45;
    public GameObject owner;
    public bool facingRight = true;
    public int activeFrames = 2;
    public int minimumHitstunFrames = 20;
    public bool cannotClank = false;




    [Tooltip("Set this to true if the hitbox has multiple hitboxes that are part of the same move as it, and are disabled if it connects. In this case, this object should also be parented to a another gameobject. See Ghost UpHeavy. This is checked by HitboxInteractionManager.")]
    public bool isPartOfMultipart = false;

    public int hitstopFrames = 10;

    public bool isCounter = false;

    public bool isProjectile = false;

    public bool isGrab = false;

    public bool isDestroyable = false;

    [Tooltip("If isDestroyable is true, when colliding with a hitbox, reduce health by hitbox damage, and self destruct if health < 0.")]
    public float objectHealth = 0;

    public float DiMultiplier = 1;

    [Tooltip("Only used if angle is 361; where relative to attacker should the hitbox aim to move the hit player?")]
    public Vector2 knockbackGoalPos;

    [Tooltip("LOWER number priority wins; lower priority hitboxes are disabled if two hitboxes from the same move hit a player simultaneously.")]
    public int priority = 1;

    [Tooltip("Set true in all connected hitboxes when a hitbox clanks so that the rest of the hitboxes activated afterward in the same move are not activated.")]
    public bool doNotEnable = false;

    public List<GameObject> playersHitAlready;


    Collider2D hbc;
    HitboxInteractionManager him;
    private void Awake()
    {
        hbc = GetComponent<Collider2D>();
        if (transform.root.GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("Hitbox " + gameObject.name + " on object " + transform.root.name + " has a HitboxInfo script but root has no rigidbody2D! Collisions will be ignored!");
        }
        him = Camera.main.GetComponent<HitboxInteractionManager>();
    }

    public static bool ComparePair(Pair<Collider2D, Collider2D> one, Pair<Collider2D, Collider2D> two)
    {
        if ((one.left == two.left && one.right == two.right) || (one.left == two.right && one.right == two.left))
        {
            return true;
        }
        return false;
    }

    public static bool ContainsPair(Pair<Collider2D, Collider2D> pair, List<Pair<Collider2D, Collider2D>> list)
    {
        foreach (Pair<Collider2D, Collider2D> i in list)
        {
            if (ComparePair(pair, i))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pair<Collider2D, Collider2D> toSend = new Pair<Collider2D, Collider2D>(hbc, collision);

        if (collision.TryGetComponent(out HitboxInfo hbi)) // if collided with hitbox
        {
            if (hbi.owner != owner)
            {
                if (hbi.isCounter) // is normal hitbox that hit a counter
                {
                    if (!ContainsPair(toSend, him.hitboxCounterCol))
                    {
                        him.hitboxCounterCol.Add(toSend);
                    }
                }
                else // is normal hitbox that hit another hitbox
                {
                    if (!ContainsPair(toSend, him.hitboxHitboxCol))
                    {
                        him.hitboxHitboxCol.Add(toSend);
                    }
                }
            }
        }

        if (collision.TryGetComponent(out Control1 _)) // collided with player
        {
            if (owner != collision.gameObject)
            {
                if (isGrab) // is grab that hit player
                {
                    if (!ContainsPair(toSend, him.grabPlayerCol))
                    {
                        him.grabPlayerCol.Add(toSend);
                    }
                }
                else if (!isCounter)// is normal hitbox that hit player
                {
                    if (!ContainsPair(toSend, him.hitboxPlayerCol))
                    {
                        him.hitboxPlayerCol.Add(toSend);
                    }
                }
            }
        }
    }
}
