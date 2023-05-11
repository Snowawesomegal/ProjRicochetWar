using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StanleyAnimationEvents : MonoBehaviour
{
    Control1 c1;
    Animator anim;
    AnimationEvents ae;
    Rigidbody2D rb;
    [SerializeField] GameObject paperPrefab;
    [SerializeField] GameObject monitorPrefab;
    [SerializeField] float monitorLifetime = 10;
    [SerializeField] float monitorSpeed = 100;
    public int activeMonitors = 0;

    [SerializeField] List<StanleyPaperInfo> stanleyPaperInfos;
    [SerializeField] float stanleyPaperSpeed;
    [SerializeField] float stanleyPaperLifetime;

    Vector2 magnetTargetDistance = new Vector2 (666, 666);
    [SerializeField] float magnetLaunchSpeed = 1000;


    private void Start()
    {
        c1 = GetComponent<Control1>();
        anim = GetComponent<Animator>();
        ae = GetComponent<AnimationEvents>();
        rb = GetComponent<Rigidbody2D>();
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

    [System.Serializable]
    public class StanleyPaperInfo
    {
        public Vector2 offset;
        public float angle;
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

    public void StartMagnetTracking()
    {
        StartCoroutine(nameof(MagnetTracker));
    }

    public void StopMagnetTracking()
    {
        StopCoroutine(nameof(MagnetTracker));
        ae.ChangeAnimBool("ContinueAttack", true);
        c1.ignoreFriction = true;
        c1.affectedByGravity = false;
        rb.AddForce(magnetTargetDistance.normalized * magnetLaunchSpeed);
    }

    IEnumerator MagnetTracker()
    {
        float direction = c1.facingRight ? 1 : -1;

        List<GameObject> playerList = GameObject.FindGameObjectsWithTag("Player").ToList();

        playerList.Remove(gameObject);

        magnetTargetDistance = new Vector2(666, 666);

        while (true)
        {
            float xInput = c1.pim.GetCurrentDirectional().current.x;

            if (xInput != 0)
            {
                direction = xInput;
            }

            List<GameObject> playerListCopy = new List<GameObject>(playerList);
            foreach (GameObject player in playerListCopy)
            {
                if (player == null)
                {
                    playerList.Remove(player);
                    continue;
                }

                if (player.transform.position.x > transform.position.x == direction > 0)
                {
                    Vector2 tempDistance = player.transform.position - transform.position;
                    if (tempDistance.magnitude < magnetTargetDistance.magnitude)
                    {
                        magnetTargetDistance = tempDistance;
                    }
                }
            }

            anim.SetFloat("HorizontalDistance", magnetTargetDistance.normalized.x);
            anim.SetFloat("VerticalDistance", magnetTargetDistance.normalized.y);

            yield return null;
        }
    }

}
