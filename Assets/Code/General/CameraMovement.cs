using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    List<GameObject> toFollow = new List<GameObject>();

    [SerializeField] float highestX = 666;
    [SerializeField] float lowestX = -666;
    [SerializeField] float highestY = 666;
    [SerializeField] float lowestY = -666;


    [SerializeField] float baseCameraSpeed = 10;
    [SerializeField] float reduceMoveSpeedScaling = 10;

    [SerializeField] float baseZoomSpeed = 10;
    [SerializeField] float reduceZoomSpeedScaling = 10;

    [SerializeField] float minCameraSize = 3;
    public float maxCameraSize = 10;

    [SerializeField] float zoomSpaceScale = 0.7f;

    private Camera cam;

    void Start()
    {

        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!toFollow.Contains(i))
            {
                toFollow.Add(i);
            }
        }

        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (toFollow.Count == 1)
        {
            Vector2 position2D = new Vector2(transform.position.x, transform.position.y);

            float distance = Vector2.Distance(toFollow[0].transform.position, position2D);
            float scale = distance / reduceMoveSpeedScaling; // what percent of toScaleBy is distance? Multiply speed by that

            Vector2 movement = Vector2.MoveTowards(transform.position, toFollow[0].transform.position, Time.deltaTime * scale * baseCameraSpeed);
            transform.position = new Vector3(Mathf.Clamp(movement.x, lowestX, highestX), Mathf.Clamp(movement.y, lowestY, highestY), -10);

            cam.orthographicSize = (minCameraSize + maxCameraSize) / 2;

        }
        if (toFollow.Count >= 2)
        {
            Vector2 centerpoint = GetCenter();
            float farthestDistance = GetFarthestDistance();

            Vector2 position2D = new Vector2(transform.position.x, transform.position.y);

            float distance = Vector2.Distance(centerpoint, position2D);
            float scale = distance / reduceMoveSpeedScaling; // what percent of toScaleBy is distance? Multiply speed by that

            Vector2 movement = Vector2.MoveTowards(transform.position, centerpoint, Time.deltaTime * scale * baseCameraSpeed);
            transform.position = new Vector3(Mathf.Clamp(movement.x, lowestX, highestX), Mathf.Clamp(movement.y, lowestY, highestY), -10);

            float zScale = farthestDistance / reduceZoomSpeedScaling;
            cam.orthographicSize = Mathf.MoveTowards(
                cam.orthographicSize, Mathf.Clamp(farthestDistance * zoomSpaceScale, minCameraSize, maxCameraSize), Time.deltaTime * baseZoomSpeed * zScale);


            Vector2 GetCenter()
            {
                Vector2 average = Vector2.zero;
                foreach (GameObject i in toFollow)
                {
                    average += new Vector2(i.transform.position.x, i.transform.position.y);
                }
                average /= toFollow.Count;

                return average;
            }

            float GetFarthestDistance()
            {
                float farthest = 1;
                foreach (GameObject i in toFollow)
                {
                    foreach (GameObject j in toFollow)
                    {
                        float current = Vector2.Distance(i.transform.position, j.transform.position);
                        if (Mathf.Abs(current) > farthest)
                        {
                            farthest = Mathf.Abs(current);
                        }
                    }
                }
                return farthest;

            }
        }
    }
}
