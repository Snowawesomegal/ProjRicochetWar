using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pogobox : MonoBehaviour
{
    Rigidbody2D playerrb;
    public GameObject owner;
    AudioManager am;
    public bool facingRight = true;

    [SerializeField] float bounceForce = 1300;

    [Tooltip("Degrees, 0 is directly to the right, 90 is straight up.")]
    [SerializeField] float bounceAngle = 90;

    [SerializeField] string soundOnBounce;

    public int activeFrames = 2;

    private void Awake()
    {
        owner = transform.root.gameObject;
        playerrb = owner.GetComponent<Rigidbody2D>();
        am = owner.GetComponent<Control1>().am;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Standable"))
        {
            playerrb.velocity = new Vector2(playerrb.velocity.x, 0);
            playerrb.AddForce(AngleMath.Vector2FromAngle(bounceAngle).normalized * bounceForce);

            if (!string.IsNullOrEmpty(soundOnBounce))
            {
                am.PlaySound(soundOnBounce);
            }

            gameObject.SetActive(false);
        }
    }
}
