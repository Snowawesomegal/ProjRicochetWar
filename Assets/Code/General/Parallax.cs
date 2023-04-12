using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] float parallaxScale;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.transform.position / parallaxScale;
    }
}
