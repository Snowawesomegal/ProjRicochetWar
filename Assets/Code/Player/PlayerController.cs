using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moving = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
    private void FixedUpdate()
    {
        // Debug.Log("here");
        rb.velocity = new Vector2(moving, 0);
    }

    public void SetHorizontal(float moving)
    {
        this.moving = moving;
    }
}
