using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyAnimationEvents : MonoBehaviour
{
    Control1 c1;
    [SerializeField] GameObject paperPrefab;
    [SerializeField] GameObject monitorPrefab;
    [SerializeField] float monitorLifetime = 10;
    [SerializeField] float monitorSpeed = 100;
    public int activeMonitors = 0;

    [SerializeField] List<StanleyPaperInfo> stanleyPaperInfos;
    [SerializeField] float stanleyPaperSpeed;
    [SerializeField] float stanleyPaperLifetime;


    private void Start()
    {
        c1 = GetComponent<Control1>();
    }

    public void SpawnPaper(int index)
    {
        Vector2 angleOfForce = AngleMath.Vector2FromAngle(stanleyPaperInfos[index].angle);

        angleOfForce = angleOfForce.normalized;

        GameObject newStanleyPaper = Instantiate(paperPrefab, transform.position + (Vector3) stanleyPaperInfos[index].offset, Quaternion.identity);

        newStanleyPaper.GetComponent<Rigidbody2D>().AddForce(angleOfForce * stanleyPaperSpeed);
        newStanleyPaper.transform.GetChild(0).GetComponent<HitboxInfo>().owner = gameObject;
        Destroy(newStanleyPaper, stanleyPaperLifetime);
    }

    public void SpawnMonitor()
    {
        if (activeMonitors == 0)
        {
            Vector2 angleOfForce = c1.pim.GetCurrentDirectional().current;

            if (angleOfForce == Vector2.zero)
            {
                angleOfForce = new Vector2(1 * (c1.facingRight ? 1 : -1), 0);
            }

            angleOfForce = angleOfForce.normalized;

            GameObject newMonitor = Instantiate(monitorPrefab, transform.position, Quaternion.identity);

            newMonitor.GetComponent<Rigidbody2D>().AddForce(angleOfForce * monitorSpeed);
            newMonitor.transform.GetChild(0).GetComponent<HitboxInfo>().owner = gameObject;
            newMonitor.GetComponent<StanleyMonitorControl>().Invoke("ManualSelfDestruct", monitorLifetime);

            activeMonitors += 1;
        }
    }

    [System.Serializable]
    public class StanleyPaperInfo
    {
        public Vector2 offset;
        public float angle;
    }
}
