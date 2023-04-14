using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    public float scrollSpeed;

    private Renderer rend;
    private Vector2 savedOffset;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.sortingLayerName = "-3";
    }

    void Update()
    {
        float x = Mathf.Repeat(Time.time * scrollSpeed, 1);
        Vector2 offset = new Vector2(x, 0);
        rend.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }
}
