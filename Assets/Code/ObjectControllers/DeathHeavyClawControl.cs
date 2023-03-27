using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class DeathHeavyClawControl : MonoBehaviour
{
    HitboxInteractionManager him;
    CapsuleCollider2D cc;
    GameObject hitboxObject;
    AudioManager am;

    public GameObject owner = null;

    public GameObject grabbedPlayer = null;
    public Rigidbody2D grabbedPlayerRB = null;

    Transform grabPosition;

    [Tooltip("What frame and where the player should be held if they were grabbed by the move. Must be in ascending frame order.")]
    [SerializeField] List<Pair<int, Vector2>> frameAndGrabPosition;

    public int framesActive = 0;



    private void Awake()
    {
        hitboxObject = transform.GetChild(1).gameObject;
        cc = hitboxObject.GetComponent<CapsuleCollider2D>();
        am = GameObject.Find("SettingsManager").GetComponent<AudioManager>();

        if (GetComponent<SpriteRenderer>().flipX)
        {
            cc.transform.localPosition = new Vector3 (cc.transform.localPosition.x * -1, cc.transform.localPosition.y, 0);
        }

        grabPosition = transform.GetChild(0);

        him = Camera.main.GetComponent<HitboxInteractionManager>();
    }

    private void FixedUpdate()
    {
        foreach(Pair<int, Vector2> i in frameAndGrabPosition)
        {
            if (i.left < framesActive)
            {
                grabPosition.localPosition = i.right;
            }
        }

        if (grabbedPlayer != null)
        {
            grabbedPlayer.transform.position = grabPosition.position;
            grabbedPlayerRB.velocity = Vector2.zero;
        }

        framesActive += 1;
    }

    public void EnableGrabHitbox() // to be called by animation event
    {
        hitboxObject.SetActive(true);
    }

    public void DisableGrabHitbox() // to be called by animation event
    {
        hitboxObject.SetActive(false);
    }

    public void SelfDestruct()
    {
        if (grabbedPlayer != null)
        {
            grabbedPlayer.GetComponent<Control1>().Grabbed(false);
        }

        Destroy(gameObject);
        owner.GetComponent<DeathAnimationEvents>().upwardHeavyClawExists = false;
    }

    public void PlaySoundFromAnimator(string name)
    {
        am.PlaySound(name);
    }

    private void OnTriggerEnter2D(Collider2D collision) // if player touching grab hitbox is not owner, grab player.
    {
        if (collision.gameObject != owner)
        {
            if (collision.TryGetComponent(out Control1 c1))
            {
                if (!him.grabboxesAndPlayersThisFrame.Contains(new Pair<Collider2D, Collider2D>(cc, collision)))
                {
                    him.grabboxesAndPlayersThisFrame.Add(new Pair<Collider2D, Collider2D>(cc, collision));
                }
            }
        }
    }
}
