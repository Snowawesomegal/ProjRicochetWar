using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlunderbusExplosionController : MonoBehaviour
{
    public void StartBBHitboxes()
    {
        foreach (Transform i in transform.GetChild(0))
        {
            i.gameObject.SetActive(true);
        }

    }

    public void StopBBHitboxes()
    {
        foreach (Transform i in transform.GetChild(0))
        {
            i.gameObject.SetActive(false);
        }

    }
}
