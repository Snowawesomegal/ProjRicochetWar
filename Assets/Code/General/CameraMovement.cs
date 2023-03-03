using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public List<GameObject> toFollow = new List<GameObject>();

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

    public GameObject Ghost;

    void Start()
    {
        //Debug.Log("Camera has spawned the players.");
        //toFollow.Add(Instantiate(Ghost));
        //toFollow.Add(Instantiate(Ghost));

        if (toFollow.Count < 1)
        {
            Debug.Log("no objects have been added to the camera.");
        }

        cam = GetComponent<Camera>();
    }

    void LateUpdate()
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
            foreach(GameObject i in toFollow)
            {
                average += new Vector2(i.transform.position.x, i.transform.position.y);
            }
            average /= toFollow.Count;

            return average;
        }

        float GetFarthestDistance()
        {
            float farthest = 1;
            foreach(GameObject i in toFollow)
            {
                foreach(GameObject j in toFollow)
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
