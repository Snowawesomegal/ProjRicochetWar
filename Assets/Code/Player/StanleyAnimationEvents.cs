using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanleyAnimationEvents : MonoBehaviour
{
    Control1 c1;
    [SerializeField] GameObject stanleyPaperPrefab;

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

        GameObject newStanleyPaper = Instantiate(stanleyPaperPrefab, transform.position + (Vector3) stanleyPaperInfos[index].offset, Quaternion.identity);
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
}
