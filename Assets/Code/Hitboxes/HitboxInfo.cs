using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxInfo : MonoBehaviour
{
    public float damage = 10;
    public float knockback = 100;
    public bool angleIndependentOfMovement = true;
    public float angle = 0;
    public GameObject owner;
    public bool facingRight = true;
    public int activeFrames = 2;

    public bool active = true;
}
